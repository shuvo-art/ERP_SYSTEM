# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only necessary project files for restore
COPY src/ReferenceProjectApi.Core/*.csproj ./src/ReferenceProjectApi.Core/
COPY src/ReferenceProjectApi.Infrastructure/*.csproj ./src/ReferenceProjectApi.Infrastructure/
COPY src/ReferenceProjectApi.Api/*.csproj ./src/ReferenceProjectApi.Api/
COPY src/Shared.Kernel/*.csproj ./src/Shared.Kernel/

# Restore dependencies for ReferenceProjectApi.Api
RUN dotnet restore src/ReferenceProjectApi.Api/ReferenceProjectApi.Api.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/src/ReferenceProjectApi.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ReferenceProjectApi.Api.dll"]
