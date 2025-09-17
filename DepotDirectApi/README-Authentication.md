# DepotDirect API Authentication Guide

This API supports multiple authentication methods to provide flexibility for different client applications and use cases.

## Supported Authentication Methods

### 1. Basic Authentication
Simple username/password authentication using HTTP Basic Auth.

**Test Credentials:**
- **Admin User:** username: `admin`, password: `admin123`
- **Regular User:** username: `user`, password: `user123`

**Usage:**
```
Authorization: Basic YWRtaW46YWRtaW4xMjM=
```

### 2. JWT Bearer Token Authentication
JSON Web Token-based authentication for stateless API access.

**Steps:**
1. Login via `/auth/login` endpoint
2. Receive JWT token in response
3. Use token in subsequent requests

**Usage:**
```
Authorization: Bearer YOUR_JWT_TOKEN_HERE
```

### 3. OAuth 2.0 Authentication
Support for external OAuth providers including Google and Microsoft.

**Available Providers:**
- Google OAuth 2.0
- Microsoft Account OAuth 2.0
- Custom OpenID Connect providers

## API Endpoints

### Authentication Endpoints

#### Login (Username/Password)
```
POST /auth/login
Content-Type: application/json

{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "tokenType": "Bearer",
  "expiresAt": "2024-01-01T12:00:00Z",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@depotdirect.com",
    "roles": ["Admin", "User"]
  }
}
```

#### OAuth 2.0 Login
```
GET /auth/google          # Initiate Google OAuth flow
GET /auth/microsoft       # Initiate Microsoft OAuth flow
```

### Protected Endpoints

#### Get Weather Forecast (Requires Authentication)
```
GET /weatherforecast
Authorization: Basic YWRtaW46YWRtaW4xMjM=
# OR
Authorization: Bearer YOUR_JWT_TOKEN
```

#### Get User Profile (Requires Authentication)
```
GET /profile
Authorization: Basic YWRtaW46YWRtaW4xMjM=
# OR
Authorization: Bearer YOUR_JWT_TOKEN
```

#### Admin Endpoints (Requires Admin Role)
```
GET /admin/users
Authorization: Basic YWRtaW46YWRtaW4xMjM=
# OR
Authorization: Bearer YOUR_ADMIN_JWT_TOKEN
```

### Public Endpoints

#### Public Status (No Authentication Required)
```
GET /public/status
```

## Configuration

### JWT Settings (appsettings.json)
```json
{
  "Jwt": {
    "SecretKey": "your-super-secret-key-that-is-at-least-32-characters-long-for-production-use!",
    "Issuer": "DepotDirectApi",
    "Audience": "DepotDirectApi",
    "ExpirationMinutes": 60
  }
}
```

### OAuth 2.0 Settings (appsettings.json)
```json
{
  "OAuth": {
    "Google": {
      "ClientId": "your-google-client-id",
      "ClientSecret": "your-google-client-secret"
    },
    "Microsoft": {
      "ClientId": "your-microsoft-client-id",
      "ClientSecret": "your-microsoft-client-secret"
    }
  }
}
```

## Testing Authentication

### Using PowerShell/cURL

1. **Test Public Endpoint:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5205/public/status" -Method GET
```

2. **Login and Get Token:**
```powershell
$loginData = @{
    username = "admin"
    password = "admin123"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "http://localhost:5205/auth/login" -Method POST -Body $loginData -ContentType "application/json"
$token = $response.token
```

3. **Use JWT Token:**
```powershell
$headers = @{
    "Authorization" = "Bearer $token"
}
Invoke-RestMethod -Uri "http://localhost:5205/weatherforecast" -Method GET -Headers $headers
```

4. **Use Basic Auth:**
```powershell
$credentials = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("admin:admin123"))
$headers = @{
    "Authorization" = "Basic $credentials"
}
Invoke-RestMethod -Uri "http://localhost:5205/weatherforecast" -Method GET -Headers $headers
```

### Using HTTP Files (VS Code REST Client)
See `api-tests.http` file for comprehensive test cases.

## Security Features

1. **Multiple Authentication Schemes:** Basic Auth, JWT, OAuth 2.0
2. **Role-Based Authorization:** Admin and User roles
3. **Secure Token Generation:** Using HMAC-SHA256 for JWT signing
4. **Password Hashing:** SHA256 with salt (demo - use BCrypt in production)
5. **Token Validation:** Comprehensive JWT validation with expiration
6. **HTTPS Redirection:** Enforced in production environments

## Production Considerations

1. **Replace Test Users:** Implement proper user management with database
2. **Use BCrypt:** Replace SHA256 with BCrypt for password hashing
3. **Secure Secrets:** Store JWT secret and OAuth credentials securely
4. **Configure OAuth:** Set up proper OAuth applications with correct redirect URIs
5. **HTTPS Only:** Ensure all authentication happens over HTTPS
6. **Rate Limiting:** Implement rate limiting for authentication endpoints
7. **Audit Logging:** Log authentication attempts and failures

## Swagger/OpenAPI Integration

The API includes Swagger documentation with authentication support:
- Visit `/swagger` to access the interactive API documentation
- Use the "Authorize" button to test authentication
- Supports both Basic Auth and Bearer Token authentication

## Error Handling

The API returns appropriate HTTP status codes:
- `200 OK`: Successful authentication/request
- `401 Unauthorized`: Missing or invalid credentials
- `403 Forbidden`: Valid credentials but insufficient permissions
- `400 Bad Request`: Invalid request format
- `404 Not Found`: Resource not found
- `500 Internal Server Error`: Server-side errors