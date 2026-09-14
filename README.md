# Multi-Warehouse Inventory Management System

A scalable, multi-tenant warehouse inventory management system built with .NET 8.0 and React. The system supports multi-warehouse stock management, product cataloging, intelligent order allocation, real-time stock tracking, and isolated data access across tenants.

## Features

- **Multi-Tenancy**: Complete data isolation across tenants with role-based access control
- **Multi-Warehouse Management**: Manage multiple warehouses with real-time inventory tracking
- **Intelligent Order Allocation**: Automatic warehouse selection based on customer proximity and stock availability
- **Stock Reservation System**: Hold stock for allocated orders with automatic release mechanisms
- **Real-time Notifications**: Instant order status updates via SignalR
- **Stock Documents**: Inbound/outbound stock management with automatic inventory updates
- **Warehouse Transfers**: Move stock between warehouses
- **Stocktakes**: Periodic inventory counting and adjustment
- **Warehouse Zoning**: Organize warehouses into zones for efficient storage
- **Financial Reports**: Warehouse financial summaries and analytics

## Tech Stack

### Backend
- **.NET 8.0** - Core framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 9.0** - ORM
- **MySQL** - Database
- **Redis** - Distributed caching
- **SignalR** - Real-time communication
- **JWT Bearer Authentication** - Security
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **Swagger/OpenAPI** - API documentation

### Frontend
- **React 19** - UI framework
- **TypeScript** - Type-safe JavaScript
- **Vite** - Build tool
- **Ant Design** - Component library
- **TailwindCSS** - Styling
- **Axios** - HTTP client
- **SignalR Client** - Real-time updates

## Architecture

The system follows Clean Architecture (DDD) with clear separation of concerns:

```
├── Domain/              # Business logic and entities
├── Application/        # Application services and DTOs
├── Infrastructure/      # Data access and external services
└── API/                # Controllers and API endpoints
```

## Documentation

- [Architecture Diagram](ARCHITECTURE.md) - System architecture and design patterns
- [Database Schema](DATABASE.md) - Database structure and relationships
- [API Documentation](API_DOCUMENTATION.md) - REST API endpoints
- [How to Run](HOW_TO_RUN.md) - Setup and running instructions
- [Docker Setup](DOCKER.md) - Containerization guide
- [Authentication Flow](AUTHENTICATION_FLOW.md) - Security and authentication
- [Caching Strategy](CACHING_STRATEGY.md) - Redis caching implementation
- [Concurrency Handling](CONCURRENCY_HANDLING.md) - Concurrency control mechanisms

## Quick Start

### Prerequisites
- .NET 8.0 SDK
- Node.js 20+
- MySQL 8.0+
- Redis server
- pnpm 11+

### Backend Setup

```bash
cd backend
dotnet restore
dotnet build
dotnet run --project MultiWarehouseInventory.API
```

### Frontend Setup

```bash
cd frontend
pnpm install
pnpm dev
```

## Environment Configuration

See [`.env.example`](.env.example) for required environment variables.

## API Documentation

Swagger UI is available at `http://localhost:5000/swagger` when the backend is running.

## CI/CD

- **GitHub Actions**: Automated CI pipeline for building and testing
- **Jenkins**: Alternative CI/CD pipeline configuration included

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.

## Contact

For questions or support, please open an issue on GitHub.
