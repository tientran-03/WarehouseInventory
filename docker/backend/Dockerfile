FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy chỉ riêng thư mục backend vào thư mục làm việc hiện tại (/src)
COPY backend/ ./backend/

# Trỏ đúng đường dẫn tới file sln bên trong thư mục backend vừa copy
RUN dotnet restore backend/MultiWarehouseInventory.sln
RUN dotnet publish backend/MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MultiWarehouseInventory.API.dll"]