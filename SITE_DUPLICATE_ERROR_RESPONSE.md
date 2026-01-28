# Site Code Duplicate Error - Expected API Response

## Current Implementation

The error handling is **already implemented correctly** in the controller:

```csharp
catch (ArgumentException ex)
{
    _logger.LogWarning(ex, "Validation error creating site");
    return BadRequest(new { message = ex.Message });
}
```

## Expected API Response

When you try to create a site with a duplicate site code in the same country:

### Request
```http
POST /api/user/sites
Authorization: Bearer {your_token}
Content-Type: application/json

{
  "siteCode": "11223344",
  "siteName": "Test Site",
  "regionId": 1
}
```

### Response (HTTP 400 Bad Request)
```json
{
  "message": "Site code '11223344' already exists in this country."
}
```

## How to Test

### Using Postman or Thunder Client

1. **Create the first site:**
```http
POST https://your-api-url/api/user/sites
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

{
  "siteCode": "11223344",
  "siteName": "First Site",
  "regionId": 1
}
```

**Expected:** Status 201 Created ?

2. **Try to create duplicate:**
```http
POST https://your-api-url/api/user/sites
Authorization: Bearer {your_jwt_token}
Content-Type: application/json

{
  "siteCode": "11223344",
  "siteName": "Second Site",
  "regionId": 1
}
```

**Expected:** Status 400 Bad Request with error message ?
```json
{
  "message": "Site code '11223344' already exists in this country."
}
```

### Using cURL

```bash
# Create first site
curl -X POST https://your-api-url/api/user/sites \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "siteCode": "11223344",
    "siteName": "First Site",
    "regionId": 1
  }'

# Try to create duplicate
curl -X POST https://your-api-url/api/user/sites \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "siteCode": "11223344",
    "siteName": "Second Site",
    "regionId": 1
  }'
```

### Using JavaScript/Fetch

```javascript
try {
  const response = await fetch('https://your-api-url/api/user/sites', {
    method: 'POST',
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({
      siteCode: '11223344',
      siteName: 'Duplicate Site',
      regionId: 1
    })
  });

  if (!response.ok) {
    const error = await response.json();
    console.log('Error:', error.message);
    // Output: "Site code '11223344' already exists in this country."
    
    // Show to user
    alert(error.message);
  }
} catch (err) {
  console.error('Network error:', err);
}
```

## Frontend Implementation Example

### React Example

```jsx
const createSite = async (siteData) => {
  try {
    const response = await fetch('/api/user/sites', {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(siteData)
    });

    const data = await response.json();

    if (!response.ok) {
      // Show error message to user
      toast.error(data.message); 
      // or
      setError(data.message);
      // or
      alert(data.message);
      return;
    }

    // Success
    toast.success('Site created successfully!');
    return data;
  } catch (error) {
    console.error('Error creating site:', error);
    toast.error('Failed to create site');
  }
};
```

### Angular Example

```typescript
createSite(siteData: CreateSiteDto): Observable<SiteResponseDto> {
  return this.http.post<SiteResponseDto>('/api/user/sites', siteData)
    .pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 400 && error.error?.message) {
          // Show the error message to the user
          this.notificationService.showError(error.error.message);
        }
        return throwError(() => error);
      })
    );
}
```

### Vue Example

```javascript
async createSite(siteData) {
  try {
    const response = await this.$axios.post('/api/user/sites', siteData);
    this.$toast.success('Site created successfully!');
    return response.data;
  } catch (error) {
    if (error.response?.status === 400) {
      // Display the error message from API
      this.$toast.error(error.response.data.message);
    } else {
      this.$toast.error('Failed to create site');
    }
    throw error;
  }
}
```

## All Error Messages from Repository

The SiteRepository can return these ArgumentException messages:

| Scenario | Error Message |
|----------|---------------|
| Region not found | `"Region with ID {id} does not exist."` |
| Duplicate site code | `"Site code '{code}' already exists in this country."` |
| Site not found (assign) | `"Site with ID {id} does not exist."` |
| Region not found (assign) | `"Region with ID {id} does not exist."` |
| Already assigned | `"Site {id} is already assigned to Region {id}."` |
| Company mismatch | `"Site and Region must belong to the same company."` |

All of these will be returned as:
```json
{
  "message": "The error message here"
}
```

## Troubleshooting

If you're not seeing the error message:

### 1. Check the Raw Response
Use browser DevTools Network tab to see the actual API response:
- Status Code should be **400**
- Response body should contain `{ "message": "..." }`

### 2. Check for Middleware Interference
If you have global error handling middleware, it might be transforming the response.

### 3. Verify Frontend Error Handling
Make sure your frontend code is checking for `error.message` or `error.data.message` or `response.data.message` depending on your HTTP client.

### 4. Test with Swagger
If you have Swagger/OpenAPI enabled:
1. Go to `/swagger`
2. Find POST `/api/user/sites`
3. Try to create a duplicate site code
4. Check the response in Swagger UI

### 5. Check Logs
The controller logs the warning:
```
[WRN] Validation error creating site
System.ArgumentException: Site code '11223344' already exists in this country.
```

Look for this in your application logs to confirm the exception is being thrown.

## Response Structure Documentation

For consistency across your API, you might want to document the error response format:

### Success Response (201 Created)
```json
{
  "id": 42,
  "siteCode": "11223344",
  "siteName": "Test Site",
  ...
}
```

### Error Response (400 Bad Request)
```json
{
  "message": "Site code '11223344' already exists in this country."
}
```

### Error Response (404 Not Found)
```json
{
  "message": "Site not found"
}
```

### Error Response (500 Internal Server Error)
```json
{
  "message": "Internal server error",
  "details": "Error details if in development mode"
}
```

## Summary

? **The error handling is already implemented correctly**  
? **The error message IS being sent in the response**  
? **HTTP Status: 400 Bad Request**  
? **Response Format: `{ "message": "..." }`**  

The issue is likely in how the frontend is handling or displaying the error. Check your frontend error handling code to make sure it's reading and displaying the `message` property from the error response.
