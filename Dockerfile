# Simple approach - use pre-built Release binaries
FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app

# Copy pre-built binaries from local Release folder
COPY ElasticSearch/bin/Release/net9.0/ .

ENTRYPOINT ["dotnet", "ElasticSearch.dll"]

