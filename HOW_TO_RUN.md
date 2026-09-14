# How to Run

## Prerequisites

### Backend
- **.NET 8.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/8.0)
- **MySQL 8.0+** - [Download here](https://dev.mysql.com/downloads/mysql/)
- **Redis Server** - [Download here](https://redis.io/download) or use Docker

### Frontend
- **Node.js 20+** - [Download here](https://nodejs.org/)
- **pnpm 11+** - Install via `npm install -g pnpm@11`

## Backend Setup

### 1. Clone the Repository

```bash
git clone https://github.com/tientran-03/WarehouseInventory.git
cd WarehouseInventory
```

### 2. Configure Database

Create a MySQL database:

```sql
CREATE DATABASE MultiWarehouseDb CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### 3. Update Connection String

Edit `backend/MultiWarehouseInventory.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=3306;Database=MultiWarehouseDb;Uid=root;Pwd=your_password;SslMode=None;",
    "Redis": "localhost:6379"
  }
}
```

### 4. Run Migrations

```bash
cd backend
dotnet ef database update --project MultiWarehouseInventory.Infrastructure
```

### 5. Build and Run

```bash
dotnet build
dotnet run --project MultiWarehouseInventory.API
```

The API will be available at `http://localhost:5000`

### 6. Access Swagger UI

Open your browser and navigate to:
```
http://localhost:5000/swagger
```

## Frontend Setup

### 1. Install Dependencies

```bash
cd frontend
pnpm install
```

### 2. Configure API URL

Create `.env` file in `frontend` directory:

```env
VITE_API_URL=http://localhost:5000
```

### 3. Run Development Server

```bash
pnpm dev
```

The frontend will be available at `http://localhost:5173`

## Running with Docker

### Using Docker Compose

```bash
docker-compose up -d
```

This will start:
- MySQL database
- Redis server
- Backend API
- Frontend

Access the application at `http://localhost:5173`

### Individual Docker Containers

#### Backend

```bash
cd backend
docker build -t warehouse-inventory-api .
docker run -p 5000:8080 warehouse-inventory-api
```

#### Frontend

```bash
cd frontend
docker build -t warehouse-inventory-frontend .
docker run -p 5173:80 warehouse-inventory-frontend
```

## Default Admin User

After first run, you can login with:

- **Username**: `admin`
- **Password**: `Admin@123`

**Important**: Change the default password immediately after first login.

## Environment Variables

### Backend (.env or appsettings.json)

```env
ConnectionStrings__DefaultConnection=Server=localhost;Port=3306;Database=MultiWarehouseDb;Uid=root;Pwd=your_password;SslMode=None
ConnectionStrings__Redis=localhost:6379
JwtSettings__Secret=your_jwt_secret_key
JwtSettings__Issuer=MultiWarehouseAPI
JwtSettings__Audience=MultiWarehouseClient
JwtSettings__ExpiryMinutes=120
Email__SmtpHost=smtp.gmail.com
Email__Port=587
Email__Username=your_email@gmail.com
Email__Password=your_app_password
Cloudinary__CloudName=your_cloud_name
Cloudinary__ApiKey=your_api_key
Cloudinary__ApiSecret=your_api_secret
```

### Frontend (.env)

```env
VITE_API_URL=http://localhost:5000
```

## Troubleshooting

### Backend Issues

#### Database Connection Error
- Ensure MySQL is running
- Check connection string credentials
- Verify database exists

#### Redis Connection Error
- Ensure Redis server is running
- Check Redis connection string
- If not using Redis, you can disable caching in code

#### Port Already in Use
- Change port in `appsettings.json`:
```json
"Kestrel": {
  "Endpoints": {
    "Http": {
      "Url": "http://localhost:5001"
    }
  }
}
```

### Frontend Issues

#### Module Not Found
- Run `pnpm install` to install dependencies
- Clear cache: `pnpm store prune`

#### API Connection Error
- Verify backend is running
- Check `VITE_API_URL` in `.env`
- Check CORS settings in backend

#### Build Errors
- Delete `node_modules` and `pnpm-lock.yaml`
- Run `pnpm install` again

## Development Tips

### Hot Reload

- **Backend**: Changes auto-recompile on save
- **Frontend**: Vite provides hot module replacement

### Debugging

#### Backend
- Use Visual Studio or VS Code debugger
- Set breakpoints in controllers/services
- Check logs in console

#### Frontend
- Use browser DevTools (F12)
- React DevTools extension recommended
- Check Network tab for API calls

### Running Tests

#### Backend Tests

```bash
cd backend/MultiWarehouseInventory.Application.Tests
dotnet test
```

#### Frontend Tests

```bash
cd frontend
pnpm test
```

## Production Deployment

### Backend

1. Build for production:
```bash
dotnet publish -c Release -o ./publish
```

2. Deploy to server (IIS, Docker, Linux, etc.)

3. Update environment variables for production

4. Configure SSL/HTTPS

### Frontend

1. Build for production:
```bash
pnpm build
```

2. Deploy `dist` folder to web server (Nginx, Apache, etc.)

3. Configure reverse proxy to API

## Additional Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [React Documentation](https://react.dev/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [SignalR Documentation](https://docs.microsoft.com/aspnet/core/signalr/)
