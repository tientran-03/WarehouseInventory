FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ vào thư mục backend trong container
COPY . /src/backend/

WORKDIR /src/backend

# Restore solution
RUN dotnet restore MultiWarehouseInventory.sln

# Thêm 'backend/' vào trước đường dẫn publish vì đang đứng ở /src/backend mà cấu trúc bị lồng
RUN dotnet publish backend/MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MultiWarehouseInventory.API.dll"]