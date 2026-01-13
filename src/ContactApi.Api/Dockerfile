# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only necessary project files for restore
COPY src/ContactApi.Core/*.csproj ./src/ContactApi.Core/
COPY src/ContactApi.Infrastructure/*.csproj ./src/ContactApi.Infrastructure/
COPY src/ContactApi.Api/*.csproj ./src/ContactApi.Api/
COPY src/Shared.Kernel/*.csproj ./src/Shared.Kernel/

# Restore dependencies for ContactApi.Api
RUN dotnet restore src/ContactApi.Api/ContactApi.Api.csproj

# Copy everything else and build
COPY . .
WORKDIR /app/src/ContactApi.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

# Expose port
EXPOSE 8080

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ContactApi.Api.dll"]
