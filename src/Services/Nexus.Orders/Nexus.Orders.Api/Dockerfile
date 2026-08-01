FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/Nexus.Shared.Kernel/Nexus.Shared.Kernel.csproj", "BuildingBlocks/Nexus.Shared.Kernel/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Messaging/Nexus.Shared.Messaging.csproj", "BuildingBlocks/Nexus.Shared.Messaging/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Observability/Nexus.Shared.Observability.csproj", "BuildingBlocks/Nexus.Shared.Observability/"]
COPY ["src/Services/Nexus.Orders/Nexus.Orders.Domain/Nexus.Orders.Domain.csproj", "Services/Nexus.Orders/Nexus.Orders.Domain/"]
COPY ["src/Services/Nexus.Orders/Nexus.Orders.Application/Nexus.Orders.Application.csproj", "Services/Nexus.Orders/Nexus.Orders.Application/"]
COPY ["src/Services/Nexus.Orders/Nexus.Orders.Infrastructure/Nexus.Orders.Infrastructure.csproj", "Services/Nexus.Orders/Nexus.Orders.Infrastructure/"]
COPY ["src/Services/Nexus.Orders/Nexus.Orders.Api/Nexus.Orders.Api.csproj", "Services/Nexus.Orders/Nexus.Orders.Api/"]
RUN dotnet restore "Services/Nexus.Orders/Nexus.Orders.Api/Nexus.Orders.Api.csproj"

COPY src/ .
WORKDIR "/src/Services/Nexus.Orders/Nexus.Orders.Api"
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexus.Orders.Api.dll"]
