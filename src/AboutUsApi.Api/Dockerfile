# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only necessary project files for restore
COPY src/AboutUsApi.Core/*.csproj ./src/AboutUsApi.Core/
COPY src/AboutUsApi.Infrastructure/*.csproj ./src/AboutUsApi.Infrastructure/
COPY src/AboutUsApi.Api/*.csproj ./src/AboutUsApi.Api/
COPY src/Shared.Kernel/*.csproj ./src/Shared.Kernel/

# Restore dependencies
RUN dotnet restore src/AboutUsApi.Api/AboutUsApi.Api.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/src/AboutUsApi.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AboutUsApi.Api.dll"]
