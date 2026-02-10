# Runtime only - uses pre-published files from local build
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copy published files from local build
# First run: dotnet publish ElasticSearch.Api/ElasticSearch.Api.csproj -c Release
COPY ElasticSearch.Api/bin/Release/net9.0/publish/ .

ENTRYPOINT ["dotnet", "ElasticSearch.Api.dll"]

