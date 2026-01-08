# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy all csproj files recursively
COPY src/PartnerApi.Core/*.csproj ./src/PartnerApi.Core/
COPY src/PartnerApi.Infrastructure/*.csproj ./src/PartnerApi.Infrastructure/
COPY src/PartnerApi.Api/*.csproj ./src/PartnerApi.Api/
COPY src/Shared.Kernel/*.csproj ./src/Shared.Kernel/

RUN dotnet restore src/PartnerApi.Api/PartnerApi.Api.csproj

# Copy the rest of the code and build
COPY . .
WORKDIR /app/src/PartnerApi.Api
RUN dotnet publish -c Release -o /app/out

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PartnerApi.Api.dll"]
