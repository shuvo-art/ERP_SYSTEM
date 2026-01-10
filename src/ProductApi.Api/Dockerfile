# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only necessary project files for restore
COPY src/ProductApi.Core/*.csproj ./src/ProductApi.Core/
COPY src/ProductApi.Infrastructure/*.csproj ./src/ProductApi.Infrastructure/
COPY src/ProductApi.Api/*.csproj ./src/ProductApi.Api/
COPY src/Shared.Kernel/*.csproj ./src/Shared.Kernel/

# Restore dependencies for ProductApi.Api
RUN dotnet restore src/ProductApi.Api/ProductApi.Api.csproj

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
