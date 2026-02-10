# Runtime only - uses pre-published files from local build
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Copy published files from local build
# First run: dotnet publish ElasticSearch.Jobs/ElasticSearch.Jobs.csproj -c Release
COPY ElasticSearch.Jobs/bin/Release/net9.0/publish/ .

ENTRYPOINT ["dotnet", "ElasticSearch.Jobs.dll"]

