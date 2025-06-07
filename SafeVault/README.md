# SafeVault - Secure Authentication and Authorization

SafeVault is a secure application demonstrating robust authentication and authorization mechanisms to protect sensitive data.

## Security Features

### Authentication
- Secure password hashing using BCrypt
- JWT (JSON Web Token) based authentication
- Input validation to prevent SQL injection and XSS attacks
- Strong password policies

### Authorization
- Role-based access control (RBAC)
- Admin and User role separation
- Route protection based on user roles
- Custom authorization middleware

## User Credentials

For testing purposes, the system is seeded with two default accounts:

1. **Admin Account**
   - Username: `admin`
   - Password: `Admin@123`
   - Role: Admin

2. **User Account**
   - Username: `user`
   - Password: `User@123`
   - Role: User

## Implementation Details

### Password Security
- Passwords are hashed using BCrypt with salt
- Never stored in plain text
- Password requirements: minimum 8 characters, must include uppercase, lowercase, numbers, and special characters

### Authentication Flow
1. User enters credentials
2. Server validates input and checks credentials against the database
3. If valid, a JWT token is generated and returned
4. Client stores the token and sends it with subsequent requests
5. Server validates the token for protected routes

### Authorization System
- JWT tokens include user role claims
- Routes are protected based on roles
- Custom middleware checks user permissions
- Different UI elements are shown based on user role

## Testing

The application includes several test cases:
- Valid/Invalid login attempts
- Registration process
- Role-based access control testing
- Password security testing
- SQL injection and XSS prevention

## Project Structure

- **Controllers**: API endpoints for authentication and data access
- **Services**: Business logic for authentication and authorization
- **Middleware**: Custom authorization checks
- **Helpers**: Utility functions for security
- **Tests**: Validation of security mechanisms

## Security Best Practices Demonstrated

1. **Defense in Depth**: Multiple layers of security
2. **Principle of Least Privilege**: Users only have access to what they need
3. **Secure Password Handling**: Proper hashing and storage
4. **Input Validation**: Preventing injection attacks
5. **HTTPS**: All traffic is encrypted (when deployed)
6. **Token-based Authentication**: Stateless and scalable security
