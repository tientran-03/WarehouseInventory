
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src


COPY ["backend/MultiWarehouseInventory.sln", "./"]
COPY ["backend/MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj", "MultiWarehouseInventory.API/"]
COPY ["backend/MultiWarehouseInventory.Infrastructure/MultiWarehouseInventory.Infrastructure.csproj", "MultiWarehouseInventory.Infrastructure/"]
COPY ["backend/MultiWarehouseInventory.Application/MultiWarehouseInventory.Application.csproj", "MultiWarehouseInventory.Application/"]
COPY ["backend/MultiWarehouseInventory.Domain/MultiWarehouseInventory.Domain.csproj", "MultiWarehouseInventory.Domain/"]

RUN dotnet restore "MultiWarehouseInventory.sln"

COPY backend/ .
WORKDIR /src/MultiWarehouseInventory.API

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MultiWarehouseInventory.API.dll"]