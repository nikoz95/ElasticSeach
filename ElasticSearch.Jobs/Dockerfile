# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Update CA certificates and configure NuGet to skip SSL validation
RUN apt-get update && apt-get install -y ca-certificates && update-ca-certificates

# Configure environment to bypass SSL issues
ENV DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER=0
ENV NUGET_CERT_REVOCATION_MODE=offline
ENV DOTNET_CLI_TELEMETRY_OPTOUT=1

# Copy NuGet config first
COPY ["NuGet.Config", "./"]

# Copy solution and project files
COPY ["ElasticSearch.sln", "./"]
COPY ["ElasticSearch.Jobs/ElasticSearch.Jobs.csproj", "ElasticSearch.Jobs/"]
COPY ["ElasticSearch.Core/ElasticSearch.Core.csproj", "ElasticSearch.Core/"]

# Restore dependencies with retry
RUN dotnet nuget locals all --clear && \
    dotnet restore "ElasticSearch.Jobs/ElasticSearch.Jobs.csproj" \
    --disable-parallel \
    --ignore-failed-sources \
    --verbosity detailed || \
    dotnet restore "ElasticSearch.Jobs/ElasticSearch.Jobs.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/ElasticSearch.Jobs"
RUN dotnet build "ElasticSearch.Jobs.csproj" -c Release -o /app/build
RUN dotnet publish "ElasticSearch.Jobs.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS runtime
WORKDIR /app

# Copy published files
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ElasticSearch.Jobs.dll"]

