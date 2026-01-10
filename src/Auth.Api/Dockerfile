# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only necessary project files for restore
COPY src/Auth.Core/*.csproj ./src/Auth.Core/
COPY src/Auth.Infrastructure/*.csproj ./src/Auth.Infrastructure/
COPY src/Auth.Api/*.csproj ./src/Auth.Api/
COPY src/Shared.Kernel/*.csproj ./src/Shared.Kernel/

# Restore dependencies for Auth.Api
RUN dotnet restore src/Auth.Api/Auth.Api.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/src/Auth.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Auth.Api.dll"]
