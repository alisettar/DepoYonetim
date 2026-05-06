FROM mcr.microsoft.com/dotnet/sdk:10.0 AS base
WORKDIR /app

# Restore
COPY Directory.Build.props Directory.Packages.props ./
COPY WMS.Api/WMS.Api.csproj WMS.Api/
COPY WMS.Application/WMS.Application.csproj WMS.Application/
COPY WMS.Domain/WMS.Domain.csproj WMS.Domain/
COPY WMS.Infrastructure/WMS.Infrastructure.csproj WMS.Infrastructure/
COPY WMS.Infrastructure.Catalog/WMS.Infrastructure.Catalog.csproj WMS.Infrastructure.Catalog/
COPY WMS.Shared/WMS.Shared.csproj WMS.Shared/
RUN dotnet restore WMS.Api/WMS.Api.csproj

# Copy source and run watch
COPY . .
RUN dotnet build WMS.Api/WMS.Api.csproj

EXPOSE 5000

ENV ASPNETCORE_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "watch", "run", "--project", "WMS.Api/WMS.Api.csproj", "--urls", "http://+:5000"]
