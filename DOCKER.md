# Docker Setup

This document explains how to containerize and run the Multi-Warehouse Inventory Management System using Docker.

## Prerequisites

- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose

## Quick Start

Using Docker Compose is the easiest way to run the entire stack:

```bash
docker-compose up -d
```

This will start:
- MySQL database
- Redis server
- Backend API
- Frontend application

Access the application at `http://localhost:5173`

## Docker Compose Services

### MySQL Database

```yaml
mysql:
  image: mysql:8.0
  environment:
    MYSQL_ROOT_PASSWORD: rootpassword
    MYSQL_DATABASE: MultiWarehouseDb
  ports:
    - "3306:3306"
  volumes:
    - mysql_data:/var/lib/mysql
```

### Redis

```yaml
redis:
  image: redis:7-alpine
  ports:
    - "6379:6379"
  volumes:
    - redis_data:/data
```

### Backend API

```yaml
backend:
  build:
    context: ./backend
    dockerfile: Dockerfile
  environment:
    - ConnectionStrings__DefaultConnection=Server=mysql;Port=3306;Database=MultiWarehouseDb;Uid=root;Pwd=rootpassword;SslMode=None
    - ConnectionStrings__Redis=redis:6379
  ports:
    - "5000:8080"
  depends_on:
    - mysql
    - redis
```

### Frontend

```yaml
frontend:
  build:
    context: ./frontend
    dockerfile: Dockerfile
  environment:
    - VITE_API_URL=http://localhost:5000
  ports:
    - "5173:80"
  depends_on:
    - backend
```

## Individual Dockerfiles

### Backend Dockerfile

Create `backend/Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj", "MultiWarehouseInventory.API/"]
COPY ["MultiWarehouseInventory.Application/MultiWarehouseInventory.Application.csproj", "MultiWarehouseInventory.Application/"]
COPY ["MultiWarehouseInventory.Domain/MultiWarehouseInventory.Domain.csproj", "MultiWarehouseInventory.Domain/"]
COPY ["MultiWarehouseInventory.Infrastructure/MultiWarehouseInventory.Infrastructure.csproj", "MultiWarehouseInventory.Infrastructure/"]
RUN dotnet restore "MultiWarehouseInventory.API/MultiWarehouseInventory.API.csproj"
COPY . .
WORKDIR "/src/MultiWarehouseInventory.API"
RUN dotnet build "MultiWarehouseInventory.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MultiWarehouseInventory.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MultiWarehouseInventory.API.dll"]
```

### Frontend Dockerfile

Create `frontend/Dockerfile`:

```dockerfile
FROM node:20-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm install -g pnpm@11
RUN pnpm install
COPY . .
RUN pnpm build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
```

Create `frontend/nginx.conf`:

```nginx
server {
    listen 80;
    server_name localhost;
    root /usr/share/nginx/html;
    index index.html;

    location / {
        try_files $uri $uri/ /index.html;
    }

    location /api {
        proxy_pass http://backend:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

## Building Images

### Build Backend

```bash
cd backend
docker build -t warehouse-inventory-api .
```

### Build Frontend

```bash
cd frontend
docker build -t warehouse-inventory-frontend .
```

### Build All

```bash
docker-compose build
```

## Running Containers

### Run with Docker Compose

```bash
docker-compose up -d
```

### Run Individual Containers

#### Backend

```bash
docker run -d -p 5000:8080 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Port=3306;Database=MultiWarehouseDb;Uid=root;Pwd=rootpassword" \
  -e ConnectionStrings__Redis="host.docker.internal:6379" \
  warehouse-inventory-api
```

#### Frontend

```bash
docker run -d -p 5173:80 \
  -e VITE_API_URL="http://localhost:5000" \
  warehouse-inventory-frontend
```

## Database Migrations

Run migrations in Docker container:

```bash
docker-compose exec backend dotnet ef database update
```

Or run migrations before building:

```bash
cd backend
dotnet ef database update
```

## Environment Variables

Create `.env` file for Docker Compose:

```env
MYSQL_ROOT_PASSWORD=rootpassword
MYSQL_DATABASE=MultiWarehouseDb
JWT_SECRET=your_jwt_secret_key
REDIS_HOST=redis
MYSQL_HOST=mysql
```

Update `docker-compose.yml` to use environment variables:

```yaml
environment:
  - MYSQL_ROOT_PASSWORD=${MYSQL_ROOT_PASSWORD}
  - MYSQL_DATABASE=${MYSQL_DATABASE}
  - ConnectionStrings__DefaultConnection=Server=${MYSQL_HOST};Port=3306;Database=${MYSQL_DATABASE};Uid=root;Pwd=${MYSQL_ROOT_PASSWORD};SslMode=None
  - ConnectionStrings__Redis=${REDIS_HOST}:6379
```

## Volumes

### Data Persistence

Docker Compose uses named volumes for data persistence:

```yaml
volumes:
  mysql_data:
  redis_data:
```

This ensures data persists even if containers are removed.

### Backup Database

```bash
docker-compose exec mysql mysqldump -u root -p MultiWarehouseDb > backup.sql
```

### Restore Database

```bash
docker-compose exec -T mysql mysql -u root -p MultiWarehouseDb < backup.sql
```

## Monitoring

### View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f backend
docker-compose logs -f frontend
docker-compose logs -f mysql
```

### Container Status

```bash
docker-compose ps
```

### Resource Usage

```bash
docker stats
```

## Troubleshooting

### Container Won't Start

Check logs:
```bash
docker-compose logs backend
```

Common issues:
- Port conflicts (change ports in docker-compose.yml)
- Database connection issues (ensure MySQL is ready)
- Missing environment variables

### Database Connection Issues

Ensure backend waits for MySQL to be ready:

```yaml
backend:
  depends_on:
    mysql:
      condition: service_healthy
  healthcheck:
    test: ["CMD", "mysqladmin", "ping", "-h", "localhost"]
    interval: 10s
    timeout: 5s
    retries: 5
```

### Redis Connection Issues

Check Redis is running:
```bash
docker-compose exec redis redis-cli ping
```

Should return `PONG`.

### Frontend Build Issues

Clear Docker cache:
```bash
docker system prune -a
```

Rebuild without cache:
```bash
docker-compose build --no-cache frontend
```

## Production Deployment

### Multi-Stage Build Optimization

The provided Dockerfiles already use multi-stage builds for smaller image sizes.

### Security Best Practices

1. **Use non-root user**:
```dockerfile
FROM node:20-alpine AS build
# ... build steps ...

FROM nginx:alpine
# ... nginx config ...
RUN addgroup -g 1001 -S nodejs
RUN adduser -S nodejs -u 1001
COPY --from=build --chown=nodejs:nodejs /app/dist /usr/share/nginx/html
USER nodejs
```

2. **Scan for vulnerabilities**:
```bash
docker scan warehouse-inventory-api
docker scan warehouse-inventory-frontend
```

3. **Use specific image tags**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0.8
```

### Docker Registry

Push images to Docker Hub:

```bash
docker tag warehouse-inventory-api yourusername/warehouse-inventory-api:latest
docker push yourusername/warehouse-inventory-api:latest
```

### Orchestration

For production, consider using:
- **Kubernetes**: For large-scale deployments
- **Docker Swarm**: For simpler orchestration
- **Cloud Services**: AWS ECS, Azure Container Instances, Google Cloud Run

## Cleanup

### Stop All Containers

```bash
docker-compose down
```

### Remove Volumes

```bash
docker-compose down -v
```

### Remove All Docker Resources

```bash
docker system prune -a --volumes
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
- [.NET Docker Samples](https://github.com/dotnet/dotnet-docker)
- [React Docker Best Practices](https://mherman.org/blog/dockerizing-a-react-app/)
