using Prometheus;

namespace APICompass.KeyChecker.Infrastructure.Metrics;

public static class ValidationMetrics
{
    private static readonly Counter RequestsTotal = Prometheus.Metrics
        .CreateCounter("keychecker_validation_requests_total", "Total number of validation requests",
            new CounterConfiguration
            {
                LabelNames = new[] { "provider", "result" }
            });

    private static readonly Histogram ValidationDuration = Prometheus.Metrics
        .CreateHistogram("keychecker_validation_duration_seconds", "Duration of key validations",
            new HistogramConfiguration
            {
                LabelNames = new[] { "provider" },
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10)
            });

    private static readonly Gauge ActiveValidations = Prometheus.Metrics
        .CreateGauge("keychecker_active_validations", "Number of currently active validations",
            new GaugeConfiguration
            {
                LabelNames = new[] { "provider" }
            });

    private static readonly Counter CacheHits = Prometheus.Metrics
        .CreateCounter("keychecker_cache_hits_total", "Total number of cache hits");

    private static readonly Counter CacheMisses = Prometheus.Metrics
        .CreateCounter("keychecker_cache_misses_total", "Total number of cache misses");

    private static readonly Gauge QueueDepth = Prometheus.Metrics
        .CreateGauge("keychecker_queue_depth", "Current depth of the background job queue");

    public static void RecordValidationRequest(string provider, bool isValid)
    {
        RequestsTotal.WithLabels(provider, isValid ? "valid" : "invalid").Inc();
    }

    public static IDisposable TrackValidationDuration(string provider)
    {
        return ValidationDuration.WithLabels(provider).NewTimer();
    }

    public static void IncreaseActiveValidations(string provider)
    {
        ActiveValidations.WithLabels(provider).Inc();
    }

    public static void DecreaseActiveValidations(string provider)
    {
        ActiveValidations.WithLabels(provider).Dec();
    }

    public static void RecordCacheHit()
    {
        CacheHits.Inc();
    }

    public static void RecordCacheMiss()
    {
        CacheMisses.Inc();
    }

    public static void UpdateQueueDepth(int depth)
    {
        QueueDepth.Set(depth);
    }
}
