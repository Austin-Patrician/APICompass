# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["APICompass.KeyChecker.sln", "./"]
COPY ["src/APICompass.KeyChecker.API/APICompass.KeyChecker.API.csproj", "src/APICompass.KeyChecker.API/"]
COPY ["src/APICompass.KeyChecker.Core/APICompass.KeyChecker.Core.csproj", "src/APICompass.KeyChecker.Core/"]
COPY ["src/APICompass.KeyChecker.Validators/APICompass.KeyChecker.Validators.csproj", "src/APICompass.KeyChecker.Validators/"]
COPY ["src/APICompass.KeyChecker.Infrastructure/APICompass.KeyChecker.Infrastructure.csproj", "src/APICompass.KeyChecker.Infrastructure/"]

# Restore dependencies
RUN dotnet restore

# Copy everything else
COPY . .

# Build
WORKDIR /src/src/APICompass.KeyChecker.API
RUN dotnet build -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

# Copy published app
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "APICompass.KeyChecker.API.dll"]
