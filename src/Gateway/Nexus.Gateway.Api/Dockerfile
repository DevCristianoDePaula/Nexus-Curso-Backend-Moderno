FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/Nexus.Shared.Observability/Nexus.Shared.Observability.csproj", "BuildingBlocks/Nexus.Shared.Observability/"]
COPY ["src/Gateway/Nexus.Gateway.Api/Nexus.Gateway.Api.csproj", "Gateway/Nexus.Gateway.Api/"]
RUN dotnet restore "Gateway/Nexus.Gateway.Api/Nexus.Gateway.Api.csproj"

COPY src/ .
WORKDIR "/src/Gateway/Nexus.Gateway.Api"
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexus.Gateway.Api.dll"]
