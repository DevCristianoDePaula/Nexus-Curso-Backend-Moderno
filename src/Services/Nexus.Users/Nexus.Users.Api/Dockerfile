FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS build
WORKDIR /src

COPY ["src/BuildingBlocks/Nexus.Shared.Kernel/Nexus.Shared.Kernel.csproj", "BuildingBlocks/Nexus.Shared.Kernel/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Messaging/Nexus.Shared.Messaging.csproj", "BuildingBlocks/Nexus.Shared.Messaging/"]
COPY ["src/BuildingBlocks/Nexus.Shared.Observability/Nexus.Shared.Observability.csproj", "BuildingBlocks/Nexus.Shared.Observability/"]
COPY ["src/Services/Nexus.Users/Nexus.Users.Domain/Nexus.Users.Domain.csproj", "Services/Nexus.Users/Nexus.Users.Domain/"]
COPY ["src/Services/Nexus.Users/Nexus.Users.Application/Nexus.Users.Application.csproj", "Services/Nexus.Users/Nexus.Users.Application/"]
COPY ["src/Services/Nexus.Users/Nexus.Users.Infrastructure/Nexus.Users.Infrastructure.csproj", "Services/Nexus.Users/Nexus.Users.Infrastructure/"]
COPY ["src/Services/Nexus.Users/Nexus.Users.Api/Nexus.Users.Api.csproj", "Services/Nexus.Users/Nexus.Users.Api/"]
RUN dotnet restore "Services/Nexus.Users/Nexus.Users.Api/Nexus.Users.Api.csproj"

COPY src/ .
WORKDIR "/src/Services/Nexus.Users/Nexus.Users.Api"
RUN dotnet publish -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "Nexus.Users.Api.dll"]
