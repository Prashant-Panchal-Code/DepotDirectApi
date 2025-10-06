# Email-Based Authentication API

## Overview

I've implemented a complete email-based authentication system for your DepotDirect API. This allows frontend applications to authenticate users using their email and password.

## API Endpoints

### 1. **Login with Email and Password**
- **URL**: `POST /api/auth/login`
- **Description**: Authenticate user with email and password
- **Request Body**:
```json
{
  "email": "user@example.com",
  "password": "userpassword"
}
```
- **Success Response (200)**:
```json
{
  "success": true,
  "message": "Login successful",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "fullName": "John Doe",
    "companyId": 1,
    "companyName": "ACME Corp",
    "roleId": 2,
    "roleName": "User",
    "active": true,
    "phone": "+1234567890",
    "metadata": {},
    "createdAt": "2024-01-01T10:00:00Z",
    "updatedAt": "2024-01-01T10:00:00Z"
  }
}
```
- **Error Response (401)**:
```json
{
  "success": false,
  "message": "Invalid email or password"
}
```

### 2. **Check if Email Exists**
- **URL**: `GET /api/auth/check-email?email=user@example.com`
- **Description**: Check if a user exists with the given email
- **Response**:
```json
{
  "exists": true,
  "email": "user@example.com"
}
```

### 3. **Get User by Email (Public Info)**
- **URL**: `GET /api/auth/user-by-email?email=user@example.com`
- **Description**: Get basic user information by email (no sensitive data)
- **Response**:
```json
{
  "id": 1,
  "email": "user@example.com",
  "fullName": "John Doe",
  "companyId": 1,
  "companyName": "ACME Corp",
  "roleId": 2,
  "roleName": "User",
  "active": true
}
```

### 4. **Create Test Users (Development)**
- **URL**: `POST /api/auth/create-test-users`
- **Description**: Creates test users for development and testing
- **Response**:
```json
{
  "message": "Test user creation completed",
  "users": [
    {"email": "admin@depotdirect.com", "status": "created", "id": 1},
    {"email": "user@depotdirect.com", "status": "created", "id": 2},
    {"email": "test@example.com", "status": "created", "id": 3}
  ]
}
```

## Frontend Usage Examples

### JavaScript/TypeScript
```javascript
// Login function
async function loginUser(email, password) {
  try {
    const response = await fetch('/api/auth/login', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({ email, password })
    });

    const result = await response.json();
    
    if (result.success) {
      // Login successful
      console.log('User authenticated:', result.user);
      localStorage.setItem('user', JSON.stringify(result.user));
      return result.user;
    } else {
      // Login failed
      console.error('Login failed:', result.message);
      throw new Error(result.message);
    }
  } catch (error) {
    console.error('Login error:', error);
    throw error;
  }
}

// Check if email exists
async function checkEmailExists(email) {
  const response = await fetch(`/api/auth/check-email?email=${encodeURIComponent(email)}`);
  const result = await response.json();
  return result.exists;
}

// Usage
loginUser('user@example.com', 'password123')
  .then(user => {
    console.log('Logged in user:', user);
    // Redirect to dashboard or update UI
  })
  .catch(error => {
    console.error('Login failed:', error.message);
    // Show error message to user
  });
```

### React Hook Example
```javascript
import { useState } from 'react';

export function useAuth() {
  const [user, setUser] = useState(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const login = async (email, password) => {
    setLoading(true);
    setError(null);
    
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });

      const result = await response.json();
      
      if (result.success) {
        setUser(result.user);
        localStorage.setItem('user', JSON.stringify(result.user));
        return result.user;
      } else {
        setError(result.message);
        throw new Error(result.message);
      }
    } catch (err) {
      setError(err.message);
      throw err;
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('user');
  };

  return { user, login, logout, loading, error };
}
```

### Vue.js Composition API Example
```javascript
import { ref } from 'vue';

export function useAuth() {
  const user = ref(null);
  const loading = ref(false);
  const error = ref(null);

  const login = async (email, password) => {
    loading.value = true;
    error.value = null;
    
    try {
      const response = await fetch('/api/auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });

      const result = await response.json();
      
      if (result.success) {
        user.value = result.user;
        localStorage.setItem('user', JSON.stringify(result.user));
        return result.user;
      } else {
        error.value = result.message;
        throw new Error(result.message);
      }
    } catch (err) {
      error.value = err.message;
      throw err;
    } finally {
      loading.value = false;
    }
  };

  return { user, login, loading, error };
}
```

## Testing

### Pre-created Test Users
After running the test user creation endpoint, you'll have these users available:

1. **Admin User**
   - Email: `admin@depotdirect.com`
   - Password: `admin123`
   - Role: Admin

2. **Regular User**
   - Email: `user@depotdirect.com`
   - Password: `user123`
   - Role: User

3. **Test User**
   - Email: `test@example.com`
   - Password: `test123`
   - Role: User

### Test the API
1. First, create test users: `POST /api/auth/create-test-users`
2. Then try logging in: `POST /api/auth/login` with the test credentials
3. Check the provided `api-tests.http` file for comprehensive test cases

## Security Features

- **BCrypt Password Hashing**: Passwords are securely hashed using BCrypt
- **Input Validation**: Email format and required field validation
- **Active User Check**: Only active, non-deleted users can log in
- **Error Handling**: Comprehensive error handling and logging
- **No Password Exposure**: Login responses never include password hashes

## Implementation Details

### Files Added/Modified:
1. **IUserRepository.cs** - Added authentication method signatures
2. **UserRepository.cs** - Implemented `ValidateLoginAsync` and `GetUserEntityByEmailAsync`
3. **AuthController.cs** - New controller with login endpoints
4. **AuthDtos.cs** - DTOs for authentication requests/responses
5. **api-tests.http** - Added comprehensive test cases

### Key Methods:
- `ValidateLoginAsync()` - Validates email/password combination
- `GetUserEntityByEmailAsync()` - Retrieves user entity for authentication
- `ExistsByEmailAsync()` - Checks if email exists in database

The authentication system is now ready for your frontend to use!