# Base Solution - Architecture Overview

This repository serves as a **base solution template** for building .NET applications with authentication and security infrastructure. All business-specific logic has been removed, leaving only the essential security and infrastructure components.

## 🎯 Purpose

This is a **fork template** designed to be cloned and extended with your own business logic modules while maintaining a solid security foundation.

## 📦 What's Included

### Security Components
- **User Authentication** - Register, login, and credential verification
- **OAuth Integration** - Firebase authentication support
- **Token Management** - JWT token validation and session handling
- **Role-based Access** - User roles and permissions infrastructure

### Architecture Components
- **Clean Architecture** - Use Cases, Entities, and Interfaces separation
- **Repository Pattern** - Data access abstraction with Entity Framework Core
- **Dependency Injection** - Full DI container setup
- **API Service** - RESTful API with Swagger/OpenAPI documentation
- **Rate Limiting** - Built-in request throttling
- **CORS Configuration** - Cross-origin resource sharing setup

### Infrastructure
- **Database Context** - EF Core with SQL Server
- **Logging** - Structured logging configuration
- **Telemetry** - Application insights integration
- **Image Management** - File storage utilities
- **Shared Kernel** - Common utilities and extensions

## 🚀 Available Endpoints

### Security API (`VectorStingerSecurity.ApiService`)

```
POST /RegisterUserUseCase
- Register a new user account
- Input: email, password, user data
- Output: user details, registration status

POST /VerifyCredentialUserCase
- Verify user login credentials
- Input: email, password
- Output: authentication result, session token

POST /VerifyCredentialOAuthUseCase
- OAuth authentication (Firebase)
- Input: OAuth token
- Output: user details, session token

POST /ValidateTokenUseCase
- Validate an active session token
- Input: session token
- Output: validation result, user details
```

## 📁 Solution Structure

```
VectorStingerAPI/
├── Core.VectorStingerKernel/              # Domain layer
│   ├── Domain/
│   │   ├── DataBase/               # EF Core entities
│   │   ├── DTOs/Security/          # Security data transfer objects
│   │   └── Security/               # Security domain models
│   ├── Interfaces/
│   │   ├── Managers/Security/      # Security manager interfaces
│   │   └── Infrastructure/         # Infrastructure interfaces
│   └── Configurations/             # Core configurations
│
├── Core.VectorStingerApplication/         # Application layer
│   ├── UserCase/Security/          # Security use cases
│   │   ├── RegisterUser/
│   │   ├── VerifyCredential/
│   │   ├── VerifyCredentialOAuth/
│   │   └── ValidateToken/
│   └── Configurations/             # Application setup
│
├── VectorStinger.Modules.Security.Implement/      # Security module implementation
│   ├── Managers/                   # Security business logic
│   └── Configuration/              # Module registration
│
├── VectorStinger.Infrastructure.CoreAccessLayer/ # Data access layer
├── VectorStinger.Infrastructure.OpenAuth/        # OAuth implementation (Firebase)
├── VectorStinger.Infrastructure.PayBridge/       # Payment gateway (can be removed)
│
├── VectorStinger.Foundation.Abstraction/       # Shared interfaces
├── VectorStinger.Foundation.PlumbingProps/     # Shared utilities
│
├── VectorStingerSecurity.ApiService/      # Security API service
├── VectorStinger.ApiService/       # Main API service
├── VectorStinger.AppHost/          # Hosting configuration
├── VectorStinger.ServiceDefaults/  # Service defaults
├── VectorStinger.Web/              # Web UI (optional)
│
└── Tests/
    ├── Roomys.Infraestructure.Test/
    └── VectorStinger.Tests/
```

## 🔧 How to Extend

### Adding a New Business Module

1. **Create Module Project**
   ```bash
   dotnet new classlib -n VectorStinger.Modules.YourVectorStinger.Modules.Implement
   ```

2. **Add to Solution**
   ```bash
   dotnet sln add VectorStinger.Modules.YourVectorStinger.Modules.Implement/VectorStinger.Modules.YourVectorStinger.Modules.Implement.csproj
   ```

3. **Create Manager Interfaces** in `Core.VectorStingerKernel/Interfaces/Managers/YourModule/`

4. **Implement Managers** in `VectorStinger.Modules.YourVectorStinger.Modules.Implement/Managers/`

5. **Create Use Cases** in `Core.VectorStingerApplication/UserCase/YourModule/`

6. **Register Module** in `Core.VectorStingerApplication/Configurations/VectorStingerMain.cs`:
   ```csharp
   Assembly assemblyYourModule = typeof(YourModuleMain).Assembly;
   
   assemblyYourVectorStinger.Modules.GetTypes()
       .Where(t => t.IsClass && typeof(IManager).IsAssignableFrom(t))
       .ToList().ForEach(manager => {
           var typeManagerAbstraction = assemblyKernel.GetTypes()
               .Where(t => t.IsInterface && t.IsAssignableFrom(manager))
               .FirstOrDefault();
           if (typeManagerAbstraction != null) {
               services.AddTransient(typeManagerAbstraction, manager);
           }
       });
   ```

### Example Module Structure

```
VectorStinger.Modules.YourVectorStinger.Modules.Implement/
├── Configuration/
│   └── YourModuleMain.cs           # Module registration
├── Managers/
│   └── YourManager.cs              # Business logic implementation
└── VectorStinger.Modules.YourVectorStinger.Modules.Implement.csproj

Core.VectorStingerKernel/Interfaces/Managers/YourModule/
└── IYourManager.cs                 # Manager interface

Core.VectorStingerApplication/UserCase/YourModule/
├── DoSomething/
│   ├── DoSomethingInput.cs
│   ├── DoSomethingOutput.cs
│   ├── DoSomethingUseCase.cs
│   └── DoSomethingValidation.cs
```

## 🛠️ Technology Stack

- **.NET 9** - Latest .NET framework
- **Entity Framework Core** - ORM for database access
- **SQL Server** - Database
- **Firebase** - OAuth authentication provider
- **FluentValidation** - Input validation
- **Swagger/OpenAPI** - API documentation
- **xUnit** - Testing framework
- **Aspire** - Cloud-native orchestration

## 🔐 Security Features

- **Password Hashing** - Secure password storage
- **JWT Tokens** - Stateless authentication
- **Session Management** - User session tracking
- **Rate Limiting** - DDoS protection
- **CORS** - Controlled cross-origin access
- **HTTPS** - Encrypted communication

## 📝 Configuration

### Database Connection
Update in `appsettings.json`:
```json
{
  "DatabaseSettings": {
    "Provider": "SQLServer",
    "DefaultConnection": "Data Source=YOUR_SERVER;Initial Catalog=YOUR_DB;..."
  }
}
```

### OAuth (Firebase)
Configure Firebase credentials in your authentication provider settings.

### Folders
Configure file storage paths:
```json
{
  "Folders": [
    {
      "target": "Images",
      "path": "/path/to/images"
    }
  ]
}
```

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Roomys.Infraestructure.Test/Roomys.Infraestructure.Test.csproj
```

## 📚 Best Practices

1. **Use Case Pattern** - Each business operation is a separate use case
2. **Interface Segregation** - Small, focused interfaces
3. **Dependency Injection** - Constructor injection throughout
4. **Validation** - FluentValidation for input validation
5. **Error Handling** - Use FluentResults for operation results
6. **Logging** - Structured logging with ILogger
7. **Testing** - Unit tests for business logic

## 🚦 Getting Started

1. Clone the repository
2. Update database connection string
3. Run migrations: `dotnet ef database update`
4. Add your business modules
5. Run the application: `dotnet run`

## 📖 Documentation

For detailed setup instructions, see [README.md](README.md)

## 📄 License

This base solution is provided as a template for building new applications.

---

**Note**: This is a cleaned base solution with business logic removed. The PayBridge infrastructure is still included but can be removed if payment processing is not needed.
