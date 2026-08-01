FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/Nexus.Shared.Kernel/Nexus.Shared.Kernel.csproj", "BuildingBlocks/Nexus.Shared.Kernel/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Messaging/Nexus.Shared.Messaging.csproj", "BuildingBlocks/Nexus.Shared.Messaging/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Observability/Nexus.Shared.Observability.csproj", "BuildingBlocks/Nexus.Shared.Observability/"]
COPY ["src/Services/Nexus.Coupons/Nexus.Coupons.Domain/Nexus.Coupons.Domain.csproj", "Services/Nexus.Coupons/Nexus.Coupons.Domain/"]
COPY ["src/Services/Nexus.Coupons/Nexus.Coupons.Application/Nexus.Coupons.Application.csproj", "Services/Nexus.Coupons/Nexus.Coupons.Application/"]
COPY ["src/Services/Nexus.Coupons/Nexus.Coupons.Infrastructure/Nexus.Coupons.Infrastructure.csproj", "Services/Nexus.Coupons/Nexus.Coupons.Infrastructure/"]
COPY ["src/Services/Nexus.Coupons/Nexus.Coupons.Api/Nexus.Coupons.Api.csproj", "Services/Nexus.Coupons/Nexus.Coupons.Api/"]
RUN dotnet restore "Services/Nexus.Coupons/Nexus.Coupons.Api/Nexus.Coupons.Api.csproj"

COPY src/ .
WORKDIR "/src/Services/Nexus.Coupons/Nexus.Coupons.Api"
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexus.Coupons.Api.dll"]
