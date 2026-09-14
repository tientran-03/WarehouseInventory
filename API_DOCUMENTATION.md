# API Documentation

## Base URL
- **Development**: `http://localhost:5000`
- **Production**: `https://api.yourdomain.com`

## Authentication

All API endpoints (except authentication endpoints) require JWT Bearer token in the Authorization header:

```
Authorization: Bearer {your_jwt_token}
```

## Response Format

All responses follow this structure:

```json
{
  "success": true,
  "data": { ... },
  "message": "Operation successful",
  "errorCode": null
}
```

Error responses:

```json
{
  "success": false,
  "data": null,
  "message": "Error message",
  "errorCode": "ERROR_CODE"
}
```

## Endpoints

### Authentication

#### POST /auth/register
Register a new user.

**Request Body**:
```json
{
  "username": "string",
  "email": "string",
  "password": "string",
  "tenantCode": "string"
}
```

**Response**: `UserResponse`

#### POST /auth/login
Authenticate user and receive JWT token.

**Request Body**:
```json
{
  "username": "string",
  "password": "string"
}
```

**Response**: `LoginResponse`
```json
{
  "token": "string",
  "user": { ... }
}
```

#### POST /auth/verify-email
Verify user email address.

**Request Body**:
```json
{
  "token": "string"
}
```

#### POST /auth/change-password
Change user password.

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "currentPassword": "string",
  "newPassword": "string"
}
```

### Tenants

#### GET /tenants
Get all tenants (Admin only).

**Headers**: `Authorization: Bearer {token}`

**Response**: `List<TenantResponse>`

#### POST /tenants
Create a new tenant (Admin only).

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "name": "string",
  "code": "string"
}
```

**Response**: `TenantResponse`

### Warehouses

#### GET /warehouses
Get all warehouses for current tenant.

**Headers**: `Authorization: Bearer {token}`

**Query Parameters**:
- `activeOnly` (boolean): Filter active warehouses only

**Response**: `List<WarehouseResponse>`

#### GET /warehouses/{id}
Get warehouse by ID.

**Headers**: `Authorization: Bearer {token}`

**Response**: `WarehouseResponse`

#### POST /warehouses
Create a new warehouse.

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "name": "string",
  "code": "string",
  "latitude": 21.0285,
  "longitude": 105.8542,
  "capacity": 1000,
  "isActive": true
}
```

**Response**: `WarehouseResponse`

#### PUT /warehouses/{id}
Update warehouse.

**Headers**: `Authorization: Bearer {token}`

**Request Body**: Same as create

**Response**: `WarehouseResponse`

#### DELETE /warehouses/{id}
Delete warehouse.

**Headers**: `Authorization: Bearer {token}`

### Products

#### GET /products
Get all products for current tenant.

**Headers**: `Authorization: Bearer {token}`

**Query Parameters**:
- `categoryId` (GUID): Filter by category
- `activeOnly` (boolean): Filter active products only

**Response**: `List<ProductResponse>`

#### GET /products/{id}
Get product by ID.

**Headers**: `Authorization: Bearer {token}`

**Response**: `ProductResponse`

#### POST /products
Create a new product.

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "name": "string",
  "code": "string",
  "categoryId": "guid",
  "price": 100.00,
  "imageUrl": "string"
}
```

**Response**: `ProductResponse`

#### PUT /products/{id}
Update product.

**Headers**: `Authorization: Bearer {token}`

**Request Body**: Same as create

**Response**: `ProductResponse`

#### DELETE /products/{id}
Delete product.

**Headers**: `Authorization: Bearer {token}`

### Inventory

#### GET /inventory
Get inventory for current tenant.

**Headers**: `Authorization: Bearer {token}`

**Query Parameters**:
- `warehouseId` (GUID): Filter by warehouse
- `productId` (GUID): Filter by product

**Response**: `List<InventoryResponse>`

#### GET /inventory/warehousezoning
Get warehouse zoning information.

**Headers**: `Authorization: Bearer {token}`

**Query Parameters**:
- `warehouseId` (GUID): Filter by warehouse

**Response**: `List<WarehouseZoneResponse>`

### Orders

#### GET /orders
Get all orders for current tenant.

**Headers**: `Authorization: Bearer {token}`

**Query Parameters**:
- `status` (string): Filter by status
- `warehouseId` (GUID): Filter by warehouse

**Response**: `List<OrderResponse>`

#### GET /orders/{id}
Get order by ID with details.

**Headers**: `Authorization: Bearer {token}`

**Response**: `OrderDetailDto`

#### POST /orders
Create a new order.

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "customerName": "string",
  "customerAddress": "string",
  "customerLatitude": 21.0285,
  "customerLongitude": 105.8542,
  "items": [
    {
      "productId": "guid",
      "quantity": 10
    }
  ]
}
```

**Response**: `OrderDetailDto`

#### PATCH /orders/{id}/status
Update order status.

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "status": "Allocated"
}
```

**Response**: `OrderDetailDto`

#### POST /orders/{id}/allocate
Allocate order to warehouses.

**Headers**: `Authorization: Bearer {token}`

**Response**: `OrderDetailDto`

#### POST /orders/{id}/create-stock-documents
Create stock documents from allocated order.

**Headers**: `Authorization: Bearer {token}`

**Response**: `OrderDetailDto`

### Stock Documents

#### GET /stock-documents
Get all stock documents for current tenant.

**Headers**: `Authorization: Bearer {token}`

**Query Parameters**:
- `type` (string): Filter by type (Inbound/Outbound)
- `warehouseId` (GUID): Filter by warehouse

**Response**: `List<StockDocumentResponse>`

#### GET /stock-documents/{id}
Get stock document by ID.

**Headers**: `Authorization: Bearer {token}`

**Response**: `StockDocumentDetailResponse`

#### POST /stock-documents
Create a new stock document.

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "warehouseId": "guid",
  "type": "Inbound",
  "documentDate": "2024-01-01T00:00:00Z",
  "partnerName": "string",
  "referenceNumber": "string",
  "notes": "string",
  "lines": [
    {
      "productId": "guid",
      "warehouseZoneId": "guid",
      "quantity": 100,
      "unitPrice": 10.00
    }
  ]
}
```

**Response**: `StockDocumentDetailResponse`

### Warehouse Transfers

#### GET /transfers
Get all warehouse transfers.

**Headers**: `Authorization: Bearer {token}`

**Response**: `List<TransferResponse>`

#### POST /transfers
Create a new warehouse transfer.

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "fromWarehouseId": "guid",
  "toWarehouseId": "guid",
  "items": [
    {
      "productId": "guid",
      "quantity": 50
    }
  ]
}
```

**Response**: `TransferResponse`

#### PATCH /transfers/{id}/status
Update transfer status.

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "status": "Completed"
}
```

**Response**: `TransferResponse`

### Stocktakes

#### GET /stocktakes
Get all stocktakes.

**Headers**: `Authorization: Bearer {token}`

**Response**: `List<StocktakeResponse>`

#### POST /stocktakes
Create a new stocktake.

**Headers**: `Authorization: Bearer {token}`

**Request Body**:
```json
{
  "warehouseId": "guid",
  "stocktakeDate": "2024-01-01T00:00:00Z"
}
```

**Response**: `StocktakeResponse`

#### POST /stocktakes/{id}/complete
Complete stocktake and adjust inventory.

**Headers**: `Authorization: Bearer {token}`

**Response**: `StocktakeResponse`

### Analytics

#### GET /analytics/dashboard
Get dashboard analytics data.

**Headers**: `Authorization: Bearer {token}`

**Response**: `DashboardAnalyticsResponse`

#### GET /analytics/warehouse-financials
Get warehouse financial summaries.

**Headers**: `Authorization: Bearer {token}`

**Query Parameters**:
- `warehouseId` (GUID): Filter by warehouse

**Response**: `List<WarehouseFinancialSummary>`

## SignalR Hub

### Warehouse Orders Hub

**URL**: `/hubs/warehouse-orders`

**Authentication**: JWT Bearer token via `accessTokenFactory`

**Events**:

#### Client → Server
- `JoinTenantGroup(tenantId)`: Join tenant-specific group
- `LeaveTenantGroup(tenantId)`: Leave tenant group

#### Server → Client
- `OrderStatusUpdated(order)`: Order status changed
- `StockDocumentCreated(document)`: New stock document created
- `InventoryChanged(inventory)`: Inventory level changed

## Error Codes

| Code | Description |
|------|-------------|
| UNAUTHORIZED | Invalid or missing authentication token |
| FORBIDDEN | User lacks permission for this action |
| NOT_FOUND | Resource not found |
| BAD_REQUEST | Invalid request data |
| INSUFFICIENT_STOCK | Not enough stock available |
| VALIDATION_ERROR | Request validation failed |
| INTERNAL_ERROR | Server error |

## Rate Limiting

- **Anonymous**: 100 requests per hour
- **Authenticated**: 1000 requests per hour
- **Admin**: No limit

## Pagination

List endpoints support pagination via query parameters:

```
?page=1&pageSize=20
```

Response includes pagination metadata:

```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 100,
    "totalPages": 5
  }
}
```

## Swagger UI

Interactive API documentation available at:
- **Development**: `http://localhost:5000/swagger`
- **Production**: `https://api.yourdomain.com/swagger`
