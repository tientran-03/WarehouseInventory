
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["backend/MultiWarehouseInventory.sln", "backend/"]
COPY ["backend/MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj", "backend/MultiWarehouseInventory.API/"]
COPY ["backend/MultiWarehouseInventory.Infrastructure/MultiWarehouseInventory.Infrastructure.csproj", "backend/MultiWarehouseInventory.Infrastructure/"]

RUN dotnet restore "backend/MultiWarehouseInventory.sln"

COPY backend/ backend/
WORKDIR /src/backend/MultiWarehouseInventory.API

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MultiWarehouseInventory.API.dll"]