FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ source code từ thư mục gốc vào /src của container
COPY . .

# Trỏ đúng đường dẫn tới file .sln ở ngay thư mục gốc
RUN dotnet restore MultiWarehouseInventory.sln
RUN dotnet publish backend/MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MultiWarehouseInventory.API.dll"]