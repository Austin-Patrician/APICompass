# KeyChecker API - Deployment Guide

## Quick Start

### Option 1: Run Locally (Development)

```bash
# Navigate to API project
cd src/APICompass.KeyChecker.API

# Run the application
dotnet run

# API will be available at:
# - HTTPS: https://localhost:5001
# - HTTP: http://localhost:5000
# - Swagger: https://localhost:5001/swagger
# - Metrics: https://localhost:5001/metrics
# - Health: https://localhost:5001/health
```

### Option 2: Docker Compose (Recommended for Production)

```bash
# Build and start all services (API + Redis + Prometheus + Grafana)
docker-compose up -d

# View logs
docker-compose logs -f keychecker-api

# Stop services
docker-compose down

# Access services:
# - API: http://localhost:5000
# - Swagger: http://localhost:5000/swagger
# - Metrics: http://localhost:5000/metrics
# - Prometheus: http://localhost:9090
# - Grafana: http://localhost:3000 (admin/admin)
# - Redis: localhost:6379
```

### Option 3: Docker Only

```bash
# Build the Docker image
docker build -t keychecker-api:latest .

# Run with in-memory cache
docker run -d -p 5000:8080 --name keychecker keychecker-api:latest

# Run with Redis
docker run -d --name redis redis:7-alpine
docker run -d -p 5000:8080 --name keychecker \
  --link redis:redis \
  -e ConnectionStrings__Redis=redis:6379 \
  keychecker-api:latest
```

## Configuration

### Environment Variables

```bash
# Redis Connection (optional - uses in-memory cache if not set)
ConnectionStrings__Redis=localhost:6379

# Background Workers
BackgroundWorkers__WorkerCount=8

# ASP.NET Core
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080

# Logging Level
Logging__LogLevel__Default=Information
```

### appsettings.json

Key configuration sections:

```json
{
  "ConnectionStrings": {
    "Redis": "localhost:6379"  // Leave empty to use in-memory cache
  },
  "BackgroundWorkers": {
    "WorkerCount": 8  // Number of background workers for job queue
  },
  "ValidationSettings": {
    "MaxConcurrentValidations": 10000,
    "EnableCaching": true,
    "CacheDurationSeconds": 300,
    "Providers": {
      "OpenAI": {
        "MaxConcurrent": 1500,
        "RetryCount": 5,
        "TimeoutSeconds": 30
      }
      // ... other providers
    }
  }
}
```

## Kubernetes Deployment

### 1. Create ConfigMap

```yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: keychecker-config
data:
  appsettings.Production.json: |
    {
      "ConnectionStrings": {
        "Redis": "redis-service:6379"
      },
      "BackgroundWorkers": {
        "WorkerCount": 16
      }
    }
```

### 2. Create Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: keychecker-api
spec:
  replicas: 3
  selector:
    matchLabels:
      app: keychecker-api
  template:
    metadata:
      labels:
        app: keychecker-api
    spec:
      containers:
      - name: keychecker
        image: keychecker-api:latest
        ports:
        - containerPort: 8080
        resources:
          requests:
            memory: "2Gi"
            cpu: "1000m"
          limits:
            memory: "8Gi"
            cpu: "4000m"
        env:
        - name: ConnectionStrings__Redis
          value: "redis-service:6379"
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
```

### 3. Create Service

```yaml
apiVersion: v1
kind: Service
metadata:
  name: keychecker-service
spec:
  selector:
    app: keychecker-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: LoadBalancer
```

### 4. Create Redis

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: redis
spec:
  replicas: 1
  selector:
    matchLabels:
      app: redis
  template:
    metadata:
      labels:
        app: redis
    spec:
      containers:
      - name: redis
        image: redis:7-alpine
        ports:
        - containerPort: 6379
        volumeMounts:
        - name: redis-data
          mountPath: /data
      volumes:
      - name: redis-data
        persistentVolumeClaim:
          claimName: redis-pvc
---
apiVersion: v1
kind: Service
metadata:
  name: redis-service
spec:
  selector:
    app: redis
  ports:
  - port: 6379
    targetPort: 6379
```

### 5. Deploy

```bash
kubectl apply -f k8s/configmap.yaml
kubectl apply -f k8s/redis.yaml
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml

# Check status
kubectl get pods
kubectl get services

# View logs
kubectl logs -f deployment/keychecker-api

# Scale
kubectl scale deployment keychecker-api --replicas=10
```

## Monitoring

### Prometheus Metrics

Access metrics at `/metrics` endpoint. Available metrics:

- `keychecker_validation_requests_total` - Total validation requests by provider and result
- `keychecker_validation_duration_seconds` - Validation duration histogram by provider
- `keychecker_active_validations` - Currently active validations by provider
- `keychecker_cache_hits_total` - Total cache hits
- `keychecker_cache_misses_total` - Total cache misses
- `keychecker_queue_depth` - Current background job queue depth
- `http_requests_received_total` - HTTP requests received
- `http_request_duration_seconds` - HTTP request duration

### Grafana Dashboards

1. Access Grafana at `http://localhost:3000` (default: admin/admin)
2. Add Prometheus data source: `http://prometheus:9090`
3. Import dashboard or create custom panels

Sample queries:

```promql
# Request rate
rate(keychecker_validation_requests_total[5m])

# Success rate
rate(keychecker_validation_requests_total{result="valid"}[5m]) / rate(keychecker_validation_requests_total[5m])

# Average validation duration
rate(keychecker_validation_duration_seconds_sum[5m]) / rate(keychecker_validation_duration_seconds_count[5m])

# Cache hit rate
rate(keychecker_cache_hits_total[5m]) / (rate(keychecker_cache_hits_total[5m]) + rate(keychecker_cache_misses_total[5m]))
```

## Performance Tuning

### 1. Connection Pooling

In `Program.cs`, HTTP clients are configured with:
- 500 connections per server
- 10-minute connection lifetime

Adjust based on load:

```csharp
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    PooledConnectionLifetime = TimeSpan.FromMinutes(10),
    MaxConnectionsPerServer = 1000  // Increase for higher throughput
})
```

### 2. Concurrency Limits

Adjust semaphore limits per provider in validator registration:

```csharp
var semaphore = new SemaphoreSlim(3000, 3000); // Increase for OpenAI
```

### 3. Background Workers

Increase worker count for batch processing:

```json
{
  "BackgroundWorkers": {
    "WorkerCount": 16  // Default: ProcessorCount * 2
  }
}
```

### 4. Redis Configuration

For high-traffic scenarios, use Redis Cluster or Sentinel:

```json
{
  "ConnectionStrings": {
    "Redis": "redis-cluster:6379,redis-cluster:6380,redis-cluster:6381"
  }
}
```

### 5. Cache Duration

Adjust based on key validation frequency:

```json
{
  "ValidationSettings": {
    "CacheDurationSeconds": 600  // 10 minutes for less volatile keys
  }
}
```

## Load Balancing

### Nginx Configuration

```nginx
upstream keychecker_backend {
    least_conn;
    server keychecker-1:8080 max_fails=3 fail_timeout=30s;
    server keychecker-2:8080 max_fails=3 fail_timeout=30s;
    server keychecker-3:8080 max_fails=3 fail_timeout=30s;
}

server {
    listen 80;
    server_name api.keychecker.com;

    location / {
        proxy_pass http://keychecker_backend;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_connect_timeout 30s;
        proxy_send_timeout 30s;
        proxy_read_timeout 30s;
    }

    location /metrics {
        deny all;  # Protect metrics endpoint
    }
}
```

## Security

### 1. API Authentication (Optional)

Add JWT or API key authentication:

```csharp
builder.Services.AddAuthentication("ApiKey")
    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>("ApiKey", options => { });
```

### 2. Rate Limiting

Already configured via `AspNetCoreRateLimit`:

```json
{
  "IpRateLimiting": {
    "EnableEndpointRateLimiting": true,
    "StackBlockedRequests": false,
    "RealIpHeader": "X-Real-IP",
    "GeneralRules": [
      {
        "Endpoint": "*",
        "Period": "1m",
        "Limit": 1000
      }
    ]
  }
}
```

### 3. HTTPS

In production, always use HTTPS:

```bash
dotnet dev-certs https --trust  # Development
```

For production, use Let's Encrypt or your certificate provider.

## Troubleshooting

### High Memory Usage

- Reduce cache duration
- Decrease concurrency limits
- Monitor with `dotnet-counters`

### Slow Response Times

- Check Prometheus metrics for bottlenecks
- Verify provider API latency
- Increase timeout values if needed
- Scale horizontally

### Redis Connection Issues

- Verify Redis is running: `redis-cli ping`
- Check connection string format
- Falls back to in-memory cache if Redis unavailable

### Build Errors

```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

## Health Checks

The API provides a health endpoint at `/health`:

```bash
curl http://localhost:5000/health
```

Returns:
- `200 OK` - Healthy
- `503 Service Unavailable` - Unhealthy

Can be integrated with:
- Kubernetes liveness/readiness probes
- Docker healthchecks
- Load balancer health checks
- Monitoring systems

## Backup and Disaster Recovery

### Redis Backup

```bash
# Backup Redis data
docker exec keychecker-redis redis-cli BGSAVE

# Copy RDB file
docker cp keychecker-redis:/data/dump.rdb ./backup/

# Restore
docker cp ./backup/dump.rdb keychecker-redis:/data/
docker restart keychecker-redis
```

### Application Logs

Logs are written to:
- Console (stdout) - captured by Docker/Kubernetes
- Files in `logs/` directory

Configure log rotation and archival based on your needs.

## Support

For issues or questions:
1. Check logs: `docker-compose logs -f`
2. Review metrics: `http://localhost:5000/metrics`
3. Monitor health: `http://localhost:5000/health`
4. Check Prometheus: `http://localhost:9090`
5. Open an issue on GitHub

## License

See LICENSE file for details.
