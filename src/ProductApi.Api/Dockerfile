# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all csproj files and restore as distinct layers
COPY src/Auth.Core/*.csproj ./src/Auth.Core/
COPY src/Auth.Infrastructure/*.csproj ./src/Auth.Infrastructure/
COPY src/Auth.Api/*.csproj ./src/Auth.Api/
COPY src/ProductApi.Core/*.csproj ./src/ProductApi.Core/
COPY src/ProductApi.Infrastructure/*.csproj ./src/ProductApi.Infrastructure/
COPY src/ProductApi.Api/*.csproj ./src/ProductApi.Api/
COPY src/Shared.Kernel/*.csproj ./src/Shared.Kernel/
COPY *.sln ./

# Restore dependencies
RUN dotnet restore

# Copy everything else and build
COPY . .
WORKDIR /app/src/ProductApi.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ProductApi.Api.dll"]
