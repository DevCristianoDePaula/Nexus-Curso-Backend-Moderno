FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/Nexus.Shared.Kernel/Nexus.Shared.Kernel.csproj", "BuildingBlocks/Nexus.Shared.Kernel/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Messaging/Nexus.Shared.Messaging.csproj", "BuildingBlocks/Nexus.Shared.Messaging/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Observability/Nexus.Shared.Observability.csproj", "BuildingBlocks/Nexus.Shared.Observability/"]
COPY ["src/Services/Nexus.Catalog/Nexus.Catalog.Domain/Nexus.Catalog.Domain.csproj", "Services/Nexus.Catalog/Nexus.Catalog.Domain/"]
COPY ["src/Services/Nexus.Catalog/Nexus.Catalog.Application/Nexus.Catalog.Application.csproj", "Services/Nexus.Catalog/Nexus.Catalog.Application/"]
COPY ["src/Services/Nexus.Catalog/Nexus.Catalog.Infrastructure/Nexus.Catalog.Infrastructure.csproj", "Services/Nexus.Catalog/Nexus.Catalog.Infrastructure/"]
COPY ["src/Services/Nexus.Catalog/Nexus.Catalog.Api/Nexus.Catalog.Api.csproj", "Services/Nexus.Catalog/Nexus.Catalog.Api/"]
RUN dotnet restore "Services/Nexus.Catalog/Nexus.Catalog.Api/Nexus.Catalog.Api.csproj"

COPY src/ .
WORKDIR "/src/Services/Nexus.Catalog/Nexus.Catalog.Api"
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexus.Catalog.Api.dll"]
