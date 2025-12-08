# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY src/OrderApi.Core/*.csproj ./src/OrderApi.Core/
COPY src/OrderApi.Infrastructure/*.csproj ./src/OrderApi.Infrastructure/
COPY src/OrderApi.Api/*.csproj ./src/OrderApi.Api/
COPY *.sln ./

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /app/src/OrderApi.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 8080

# Environment variables should be overridden by docker-compose or run command
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "OrderApi.Api.dll"]
