FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/Nexus.Shared.Kernel/Nexus.Shared.Kernel.csproj", "BuildingBlocks/Nexus.Shared.Kernel/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Messaging/Nexus.Shared.Messaging.csproj", "BuildingBlocks/Nexus.Shared.Messaging/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Observability/Nexus.Shared.Observability.csproj", "BuildingBlocks/Nexus.Shared.Observability/"]
COPY ["src/Services/Nexus.Cart/Nexus.Cart.Domain/Nexus.Cart.Domain.csproj", "Services/Nexus.Cart/Nexus.Cart.Domain/"]
COPY ["src/Services/Nexus.Cart/Nexus.Cart.Application/Nexus.Cart.Application.csproj", "Services/Nexus.Cart/Nexus.Cart.Application/"]
COPY ["src/Services/Nexus.Cart/Nexus.Cart.Infrastructure/Nexus.Cart.Infrastructure.csproj", "Services/Nexus.Cart/Nexus.Cart.Infrastructure/"]
COPY ["src/Services/Nexus.Cart/Nexus.Cart.Api/Nexus.Cart.Api.csproj", "Services/Nexus.Cart/Nexus.Cart.Api/"]
RUN dotnet restore "Services/Nexus.Cart/Nexus.Cart.Api/Nexus.Cart.Api.csproj"

COPY src/ .
WORKDIR "/src/Services/Nexus.Cart/Nexus.Cart.Api"
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexus.Cart.Api.dll"]
