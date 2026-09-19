FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Tạo thư mục backend trong container và copy toàn bộ nội dung vào đó
# Để cấu trúc bên trong container khớp hoàn toàn với đường dẫn trong file .sln
COPY . /src/backend/

# Di chuyển vào thư mục backend để chạy lệnh restore và publish
WORKDIR /src/backend

RUN dotnet restore MultiWarehouseInventory.sln
RUN dotnet publish MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MultiWarehouseInventory.API.dll"]