# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all csproj files recursively to maintain structure for restore
COPY src/TargetMarketApi.Core/*.csproj ./src/TargetMarketApi.Core/
COPY src/TargetMarketApi.Infrastructure/*.csproj ./src/TargetMarketApi.Infrastructure/
COPY src/TargetMarketApi.Api/*.csproj ./src/TargetMarketApi.Api/
COPY src/Shared.Kernel/*.csproj ./src/Shared.Kernel/

# Just restore the Api project (will cascade to dependencies)
RUN dotnet restore src/TargetMarketApi.Api/TargetMarketApi.Api.csproj

# Copy the rest of the code and build
COPY . .
WORKDIR /app/src/TargetMarketApi.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "TargetMarketApi.Api.dll"]
