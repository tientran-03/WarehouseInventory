FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Tạo cấu trúc thư mục backend trong container rồi copy toàn bộ source code vào đó
COPY . /src/backend/

# Chuyển vào thư mục backend để chạy restore và publish đúng với đường dẫn trong file .sln
WORKDIR /src/backend
RUN dotnet restore MultiWarehouseInventory.sln
RUN dotnet publish MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MultiWarehouseInventory.API.dll"]