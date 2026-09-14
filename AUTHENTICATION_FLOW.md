# Authentication Flow

## Overview

The system uses JWT (JSON Web Token) Bearer Authentication for securing API endpoints. Authentication is stateless, meaning the server doesn't store session information - all authentication data is contained within the token.

## Authentication Components

### JWT Token Structure

The JWT token consists of three parts:

1. **Header**: Algorithm and token type
2. **Payload**: Claims (user data, roles, expiration)
3. **Signature**: Cryptographic signature

Example decoded token:

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "sub": "user-id",
    "unique_name": "admin",
    "email": "admin@warehouse.com",
    "role": "Admin",
    "tenantId": "tenant-id",
    "exp": 1699999999,
    "iss": "MultiWarehouseAPI",
    "aud": "MultiWarehouseClient"
  }
}
```

## Authentication Flow Diagram

```
┌─────────────┐                    ┌─────────────┐                    ┌─────────────┐
│   Client    │                    │   Server    │                    │  Database   │
└─────────────┘                    └─────────────┘                    └─────────────┘
       │                                  │                                  │
       │  1. POST /auth/login            │                                  │
       │  {username, password}           │                                  │
       │─────────────────────────────────>│                                  │
       │                                  │                                  │
       │                                  │  2. Validate credentials         │
       │                                  │─────────────────────────────────>│
       │                                  │                                  │
       │                                  │  3. Return user data             │
       │                                  │<─────────────────────────────────│
       │                                  │                                  │
       │  4. Generate JWT token          │                                  │
       │<─────────────────────────────────│                                  │
       │                                  │                                  │
       │  5. Return token + user info    │                                  │
       │<─────────────────────────────────│                                  │
       │                                  │                                  │
       │  6. Store token in localStorage │                                  │
       │                                  │                                  │
       │                                  │                                  │
       │  7. API Request with token      │                                  │
       │  Authorization: Bearer {token}   │                                  │
       │─────────────────────────────────>│                                  │
       │                                  │                                  │
       │                                  │  8. Validate token                │
       │                                  │  9. Extract user info             │
       │                                  │                                  │
       │  10. Return protected data      │                                  │
       │<─────────────────────────────────│                                  │
```

## Detailed Flow

### 1. User Registration

**Endpoint**: `POST /auth/register`

**Request**:
```json
{
  "username": "john.doe",
  "email": "john@example.com",
  "password": "SecurePassword123!",
  "tenantCode": "TENANT001"
}
```

**Process**:
1. Validate input using FluentValidation
2. Check if username/email already exists
3. Hash password using BCrypt
4. Create user with default role (Staff)
5. Send verification email (optional)
6. Return user data (without password)

**Response**:
```json
{
  "success": true,
  "data": {
    "id": "user-id",
    "username": "john.doe",
    "email": "john@example.com",
    "role": "Staff",
    "isActive": true
  }
}
```

### 2. User Login

**Endpoint**: `POST /auth/login`

**Request**:
```json
{
  "username": "john.doe",
  "password": "SecurePassword123!"
}
```

**Process**:
1. Validate input
2. Find user by username
3. Verify password hash
4. Check if user is active
5. Generate JWT token with claims:
   - `sub`: User ID
   - `unique_name`: Username
   - `email`: Email address
   - `role`: User role
   - `tenantId`: Tenant ID
   - `exp`: Expiration time
6. Return token and user info

**Response**:
```json
{
  "success": true,
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "user-id",
      "username": "john.doe",
      "email": "john@example.com",
      "role": "Staff",
      "tenantId": "tenant-id"
    }
  }
}
```

### 3. Token Storage (Frontend)

Token is stored in browser localStorage:

```typescript
const STORAGE_KEYS = {
  TOKEN: 'auth_token',
  USER: 'auth_user'
}

// After successful login
localStorage.setItem(STORAGE_KEYS.TOKEN, token)
localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user))
```

### 4. Token Usage in API Requests

All subsequent API requests include the token:

```typescript
const axiosClient = axios.create({
  baseURL: API_URL
})

// Request interceptor
axiosClient.interceptors.request.use((config) => {
  const token = localStorage.getItem(STORAGE_KEYS.TOKEN)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})
```

### 5. Server-Side Token Validation

**Middleware**: JWT Bearer Authentication

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
        };
    });
```

### 6. Authorization (Role-Based)

**Controller Level**:

```csharp
[Authorize(Roles = nameof(UserRole.Admin))]
[HttpPost("tenants")]
public async Task<IActionResult> CreateTenant(CreateTenantRequest request)
{
    // Only Admin can access
}
```

**Policy-Based Authorization**:

```csharp
// Program.cs
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanManageOrders", policy =>
        policy.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Manager), nameof(UserRole.Staff)));
});

// Controller
[Authorize(Policy = "CanManageOrders")]
[HttpPatch("{id}/status")]
public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusRequest request)
{
    // Admin, Manager, and Staff can access
}
```

### 7. User Context Extraction

Extract user information from token:

```csharp
public class OrdersController : ControllerBase
{
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, UpdateOrderStatusRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        var role = User.FindFirstValue(ClaimTypes.Role);
        var tenantId = User.FindFirstValue("tenantId");
        
        // Use user context for business logic
        var result = await _orderService.UpdateStatusAsync(id, request.Status, userId);
        return Ok(result);
    }
}
```

### 8. Token Expiration Handling

**Frontend Token Refresh**:

```typescript
// Response interceptor
axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    if (error.response?.status === 401) {
      // Token expired or invalid
      localStorage.removeItem(STORAGE_KEYS.TOKEN)
      localStorage.removeItem(STORAGE_KEYS.USER)
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)
```

### 9. Logout

**Endpoint**: `POST /auth/logout` (optional - client-side logout)

**Process**:
1. Remove token from localStorage
2. Clear user context
3. Redirect to login page

```typescript
const logout = () => {
  localStorage.removeItem(STORAGE_KEYS.TOKEN)
  localStorage.removeItem(STORAGE_KEYS.USER)
  navigate('/login')
}
```

## SignalR Authentication

SignalR hubs also use JWT authentication:

```typescript
const connection = new HubConnectionBuilder()
  .withUrl(`${API_URL}/hubs/warehouse-orders`, {
    accessTokenFactory: () => localStorage.getItem(STORAGE_KEYS.TOKEN) || ''
  })
  .build()
```

Server-side hub authentication:

```csharp
[Authorize]
public class WarehouseOrdersHub : Hub
{
    public string GetConnectionId()
    {
        return Context.ConnectionId;
    }
}
```

## Security Best Practices

### 1. Token Storage
- ✅ Store in httpOnly cookies (more secure)
- ⚠️ Store in localStorage (easier, but vulnerable to XSS)
- ❌ Never store in sessionStorage (cleared on tab close)

### 2. Token Expiration
- Set reasonable expiration time (2 hours default)
- Implement refresh tokens for long-lived sessions
- Invalidate tokens on password change

### 3. Secret Key
- Use strong, random secret key (256 bits minimum)
- Store in environment variables (never in code)
- Rotate secret key periodically

### 4. HTTPS
- Always use HTTPS in production
- Never transmit tokens over HTTP
- Enable HSTS headers

### 5. Token Validation
- Validate issuer and audience
- Check token expiration
- Verify signature

## Multi-Tenancy Security

### Tenant Isolation

Each user is associated with a tenant:

```csharp
public class User : BaseEntity
{
    public Guid TenantId { get; set; }
    // ... other properties
}
```

### Tenant Filtering

All queries automatically filter by tenant:

```csharp
public async Task<IReadOnlyList<Warehouse>> GetByTenantAsync(Guid tenantId)
{
    return await _context.Warehouses
        .Where(w => w.TenantId == tenantId)
        .ToListAsync();
}
```

### Cross-Tenant Access Prevention

Middleware ensures users can only access their tenant's data:

```csharp
public class TenantAuthorizationMiddleware
{
    public async Task InvokeAsync(HttpContext context, IUserContext userContext)
    {
        var userTenantId = userContext.TenantId;
        var requestedTenantId = GetRequestedTenantId(context);
        
        if (userTenantId != requestedTenantId)
        {
            context.Response.StatusCode = 403;
            return;
        }
        
        await _next(context);
    }
}
```

## Error Handling

### 401 Unauthorized
- Invalid or missing token
- Token expired
- Token signature invalid

### 403 Forbidden
- User lacks required role
- User cannot access requested resource
- Cross-tenant access attempt

## Configuration

### JWT Settings (appsettings.json)

```json
{
  "JwtSettings": {
    "Secret": "your-256-bit-secret-key",
    "Issuer": "MultiWarehouseAPI",
    "Audience": "MultiWarehouseClient",
    "ExpiryMinutes": 120
  }
}
```

### CORS Configuration

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

## Testing Authentication

### Using Swagger

1. Navigate to `/swagger`
2. Click "Authorize" button
3. Enter JWT token (without "Bearer " prefix)
4. All requests will include the token

### Using cURL

```bash
# Login
curl -X POST http://localhost:5000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin@123"}'

# Use token
curl -X GET http://localhost:5000/warehouses \
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## Troubleshooting

### Token Not Working
- Check token is not expired
- Verify secret key matches between generation and validation
- Ensure token is sent in correct header format

### CORS Issues
- Verify frontend URL is in CORS allowed origins
- Check credentials are allowed
- Ensure HTTPS is used in production

### Role-Based Authorization Not Working
- Verify role claim is in token
- Check role name matches enum values
- Ensure policy is correctly configured
