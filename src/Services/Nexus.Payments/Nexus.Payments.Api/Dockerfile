FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/Nexus.Shared.Kernel/Nexus.Shared.Kernel.csproj", "BuildingBlocks/Nexus.Shared.Kernel/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Messaging/Nexus.Shared.Messaging.csproj", "BuildingBlocks/Nexus.Shared.Messaging/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Observability/Nexus.Shared.Observability.csproj", "BuildingBlocks/Nexus.Shared.Observability/"]
COPY ["src/Services/Nexus.Payments/Nexus.Payments.Domain/Nexus.Payments.Domain.csproj", "Services/Nexus.Payments/Nexus.Payments.Domain/"]
COPY ["src/Services/Nexus.Payments/Nexus.Payments.Application/Nexus.Payments.Application.csproj", "Services/Nexus.Payments/Nexus.Payments.Application/"]
COPY ["src/Services/Nexus.Payments/Nexus.Payments.Infrastructure/Nexus.Payments.Infrastructure.csproj", "Services/Nexus.Payments/Nexus.Payments.Infrastructure/"]
COPY ["src/Services/Nexus.Payments/Nexus.Payments.Api/Nexus.Payments.Api.csproj", "Services/Nexus.Payments/Nexus.Payments.Api/"]
RUN dotnet restore "Services/Nexus.Payments/Nexus.Payments.Api/Nexus.Payments.Api.csproj"

COPY src/ .
WORKDIR "/src/Services/Nexus.Payments/Nexus.Payments.Api"
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexus.Payments.Api.dll"]
