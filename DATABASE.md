# Database Schema

## Overview

The database uses MySQL 8.0 with Entity Framework Core 9.0 as ORM. The schema follows a multi-tenant design with tenant isolation at the row level.

## Entity Relationship Diagram

```
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   Tenant    │───────│  Warehouse  │───────│ WarehouseZone│
└─────────────┘       └─────────────┘       └─────────────┘
       │                     │                     │
       │                     │                     │
       ▼                     ▼                     ▼
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│    User     │       │ WarehouseInv│───────│ StockMovement│
└─────────────┘       └─────────────┘       └─────────────┘
       │                     │                     │
       │                     │                     │
       ▼                     ▼                     ▼
┌─────────────┐       ┌─────────────┐       ┌─────────────┐
│   Product   │───────│ StockDoc   │───────│ StockReservation│
└─────────────┘       └─────────────┘       └─────────────┘
                             │
                             │
                             ▼
                     ┌─────────────┐
                     │ StockDocLine│
                     └─────────────┘
```

## Tables

### Tenants
Stores tenant information for multi-tenancy.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| Name | VARCHAR(255) | Tenant name |
| Code | VARCHAR(50) | Tenant code (unique) |
| IsActive | BOOLEAN | Active status |
| CreatedAt | DATETIME | Creation timestamp |
| UpdatedAt | DATETIME | Last update timestamp |

### Users
Stores user accounts with role-based access.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| TenantId | GUID | Foreign key to Tenants |
| Username | VARCHAR(100) | Username (unique) |
| Email | VARCHAR(255) | Email address |
| PasswordHash | VARCHAR(255) | Hashed password |
| Role | VARCHAR(50) | User role (Admin, Manager, Staff) |
| IsActive | BOOLEAN | Active status |
| CreatedAt | DATETIME | Creation timestamp |
| UpdatedAt | DATETIME | Last update timestamp |

### Warehouses
Stores warehouse information with location data.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| TenantId | GUID | Foreign key to Tenants |
| Name | VARCHAR(255) | Warehouse name |
| Code | VARCHAR(50) | Warehouse code (unique per tenant) |
| Latitude | DECIMAL(10,8) | GPS latitude |
| Longitude | DECIMAL(11,8) | GPS longitude |
| Capacity | INT | Maximum storage capacity |
| IsActive | BOOLEAN | Active status |
| CreatedAt | DATETIME | Creation timestamp |
| UpdatedAt | DATETIME | Last update timestamp |

### WarehouseZones
Organizes warehouses into zones for efficient storage.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| WarehouseId | GUID | Foreign key to Warehouses |
| Name | VARCHAR(255) | Zone name |
| Code | VARCHAR(50) | Zone code |
| Capacity | INT | Zone capacity |
| UsedCapacity | INT | Current used capacity |
| CreatedAt | DATETIME | Creation timestamp |
| UpdatedAt | DATETIME | Last update timestamp |

### Products
Stores product catalog information.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| TenantId | GUID | Foreign key to Tenants |
| Name | VARCHAR(255) | Product name |
| Code | VARCHAR(50) | Product code (unique per tenant) |
| CategoryId | GUID | Foreign key to Categories |
| Price | DECIMAL(18,2) | Unit price |
| ImageUrl | VARCHAR(500) | Product image URL |
| IsActive | BOOLEAN | Active status |
| CreatedAt | DATETIME | Creation timestamp |
| UpdatedAt | DATETIME | Last update timestamp |

### WarehouseInventory
Tracks inventory levels per warehouse and product.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| WarehouseId | GUID | Foreign key to Warehouses |
| ProductId | GUID | Foreign key to Products |
| WarehouseZoneId | GUID | Foreign key to WarehouseZones |
| OnHandStock | INT | Physical stock on hand |
| ReservedStock | INT | Stock reserved for orders |
| AvailableStock | INT | Available stock (OnHand - Reserved) |
| CreatedAt | DATETIME | Creation timestamp |
| UpdatedAt | DATETIME | Last update timestamp |

**Indexes**:
- Composite index on (WarehouseId, ProductId)
- Index on WarehouseZoneId

### Orders
Stores customer orders with allocation status.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| TenantId | GUID | Foreign key to Tenants |
| OrderCode | VARCHAR(50) | Order code (unique) |
| CustomerName | VARCHAR(255) | Customer name |
| CustomerAddress | TEXT | Customer address |
| CustomerLatitude | DECIMAL(10,8) | Customer GPS latitude |
| CustomerLongitude | DECIMAL(11,8) | Customer GPS longitude |
| Status | VARCHAR(50) | Order status (Pending, Allocated, Processing, Shipped, Delivered, Cancelled) |
| TotalAmount | DECIMAL(18,2) | Total order amount |
| CreatedAt | DATETIME | Creation timestamp |
| UpdatedAt | DATETIME | Last update timestamp |

### OrderItems
Stores individual items within an order.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| OrderId | GUID | Foreign key to Orders |
| ProductId | GUID | Foreign key to Products |
| ProductName | VARCHAR(255) | Product name (snapshot) |
| Quantity | INT | Ordered quantity |
| UnitPrice | DECIMAL(18,2) | Unit price (snapshot) |
| TotalPrice | DECIMAL(18,2) | Total price (Quantity × UnitPrice) |

### SubOrders
Stores warehouse-specific order allocations.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| OrderId | GUID | Foreign key to Orders |
| WarehouseId | GUID | Foreign key to Warehouses |
| Status | VARCHAR(50) | Sub-order status |
| CreatedAt | DATETIME | Creation timestamp |

### SubOrderItems
Stores items within a sub-order.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| SubOrderId | GUID | Foreign key to SubOrders |
| ProductId | GUID | Foreign key to Products |
| Quantity | INT | Allocated quantity |

### StockDocuments
Stores inbound/outbound stock documents.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| TenantId | GUID | Foreign key to Tenants |
| WarehouseId | GUID | Foreign key to Warehouses |
| DocumentNumber | VARCHAR(50) | Document number (unique) |
| Type | VARCHAR(50) | Document type (Inbound, Outbound) |
| DocumentDate | DATETIME | Document date |
| ReferenceNumber | VARCHAR(50) | Reference number (PO/SO) |
| PartnerName | VARCHAR(255) | Supplier/Customer name |
| Notes | TEXT | Document notes |
| CreatedBy | GUID | User who created the document |
| CreatedAt | DATETIME | Creation timestamp |

### StockDocumentLines
Stores individual lines within a stock document.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| StockDocumentId | GUID | Foreign key to StockDocuments |
| ProductId | GUID | Foreign key to Products |
| WarehouseZoneId | GUID | Foreign key to WarehouseZones |
| Quantity | INT | Stock quantity |
| UnitPrice | DECIMAL(18,2) | Unit price |

### StockMovements
Tracks all stock movements for audit trail.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| StockDocumentId | GUID | Foreign key to StockDocuments |
| WarehouseId | GUID | Foreign key to Warehouses |
| ProductId | GUID | Foreign key to Products |
| MovementType | VARCHAR(50) | Movement type (In, Out, Transfer) |
| Quantity | INT | Movement quantity |
| Balance | INT | Balance after movement |
| CreatedAt | DATETIME | Creation timestamp |

### StockReservations
Tracks stock reservations for orders.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| OrderId | GUID | Foreign key to Orders |
| WarehouseId | GUID | Foreign key to Warehouses |
| ProductId | GUID | Foreign key to Products |
| Quantity | INT | Reserved quantity |
| CreatedAt | DATETIME | Creation timestamp |
| ReleasedAt | DATETIME | Release timestamp (nullable) |

### WarehouseTransfers
Tracks stock transfers between warehouses.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| TenantId | GUID | Foreign key to Tenants |
| FromWarehouseId | GUID | Foreign key to Warehouses (source) |
| ToWarehouseId | GUID | Foreign key to Warehouses (destination) |
| Status | VARCHAR(50) | Transfer status (Pending, InTransit, Completed, Cancelled) |
| CreatedBy | GUID | User who created the transfer |
| CreatedAt | DATETIME | Creation timestamp |
| UpdatedAt | DATETIME | Last update timestamp |

### Stocktakes
Stores inventory count records.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| TenantId | GUID | Foreign key to Tenants |
| WarehouseId | GUID | Foreign key to Warehouses |
| StocktakeDate | DATETIME | Stocktake date |
| Status | VARCHAR(50) | Stocktake status (Draft, InProgress, Completed) |
| CreatedBy | GUID | User who created the stocktake |
| CreatedAt | DATETIME | Creation timestamp |

### StocktakeLines
Stores individual stocktake line items.

| Column | Type | Description |
|--------|------|-------------|
| Id | GUID | Primary key |
| StocktakeId | GUID | Foreign key to Stocktakes |
| ProductId | GUID | Foreign key to Products |
| ExpectedQuantity | INT | Expected quantity |
| ActualQuantity | INT | Actual counted quantity |
| Variance | INT | Variance (Actual - Expected) |

## Database Constraints

### Foreign Keys
- All tables have foreign key constraints to ensure referential integrity
- Cascade delete is NOT used to prevent accidental data loss

### Unique Constraints
- Tenant.Code
- User.Username
- User.Email
- Warehouse.Code (per tenant)
- Product.Code (per tenant)
- Order.OrderCode
- StockDocument.DocumentNumber

### Indexes
- All foreign keys are indexed
- Composite indexes on frequently queried columns
- Full-text indexes on search fields (ProductName, CustomerName)

## Data Isolation

### Multi-Tenancy
- All tenant-specific tables include `TenantId` column
- Row-level security implemented at application level
- Queries automatically filter by `TenantId`

## Migration Strategy

Entity Framework Core migrations are used to manage database schema changes:

```bash
# Create migration
dotnet ef migrations add MigrationName --project MultiWarehouseInventory.Infrastructure

# Apply migration
dotnet ef database update --project MultiWarehouseInventory.Infrastructure
```
