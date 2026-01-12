# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only necessary project files for restore
COPY src/JobApi.Core/*.csproj ./src/JobApi.Core/
COPY src/JobApi.Infrastructure/*.csproj ./src/JobApi.Infrastructure/
COPY src/JobApi.Api/*.csproj ./src/JobApi.Api/
COPY src/Shared.Kernel/*.csproj ./src/Shared.Kernel/

# Restore dependencies for JobApi.Api
RUN dotnet restore src/JobApi.Api/JobApi.Api.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/src/JobApi.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "JobApi.Api.dll"]
