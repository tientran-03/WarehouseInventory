FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ thư mục gốc vào container
COPY . .

# Vì file .sln nằm ở ngay thư mục gốc của context, ta restore trực tiếp luôn
RUN dotnet restore MultiWarehouseInventory.sln
RUN dotnet publish MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MultiWarehouseInventory.API.dll"]