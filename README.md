# Base Service Solution

Esta es una solución base (fork) que contiene la infraestructura esencial para proyectos con autenticación y seguridad.

## Características

- ✅ Módulo de Seguridad (autenticación y autorización)
- ✅ Integración con Firebase Authentication (OAuth)
- ✅ Gestión de sesiones y tokens
- ✅ Registro y validación de usuarios
- ✅ Infraestructura base (Repository Pattern, Use Cases, etc.)
- ✅ Configuración de servicios (.NET 9)
- ❌ Lógica de negocio específica (removida para solución base)

## Estructura del Proyecto

```
├── Core.VectorStingerKernel/          # Dominio, interfaces y configuraciones
├── Core.VectorStingerApplication/     # Casos de uso de aplicación
│   └── UserCase/
│       └── Security/           # Casos de uso de seguridad
├── Module.Security.Implement/  # Implementación del módulo de seguridad
├── Infrastructure.*/           # Capa de infraestructura
├── SharedKernel.*/             # Componentes compartidos
├── VectorStingerSecurity.ApiService/  # API de seguridad
└── VectorStinger.ApiService/   # API principal

```

## Como arrancar el proyecto

### Requisitos previos
- .NET 9 SDK
- SQL Server (para la base de datos)

### Abrir la solución
Abra la solución desde el folder donde clonó: `/VectorStingerAPI/VectorStinger.sln`

### Primera vez - Instalar EF Tools
```bash
dotnet tool install --global dotnet-ef --version 9.*
```

### Actualizar la Base de Datos 
Ejecutar el siguiente comando desde el directorio: `./VectorStingerAPI/Core.VectorStingerKernel`

```bash
dotnet ef dbcontext scaffold "Data Source=YOUR_SERVER;Initial Catalog=YOUR_DB;Persist Security Info=False;User ID=YOUR_USER;Password=YOUR_PASSWORD;Trust Server Certificate=True" Microsoft.EntityFrameworkCore.SqlServer --context-dir Domain\DataBase\Data --output-dir Domain\DataBase\Models --force
```

**Nota:** Actualice la cadena de conexión con sus propios valores de servidor, base de datos, usuario y contraseña.

## Compilar y Ejecutar

```bash
# Compilar la solución
dotnet build

# Ejecutar las pruebas
dotnet test

# Ejecutar el servicio de seguridad
dotnet run --project VectorStingerSecurity.ApiService

# Ejecutar el servicio principal
dotnet run --project VectorStinger.ApiService
```

## Endpoints de Seguridad Disponibles

- `POST /RegisterUserUseCase` - Registrar nuevo usuario
- `POST /VerifyCredentialUserCase` - Verificar credenciales de usuario
- `POST /VerifyCredentialOAuthUseCase` - Verificar credenciales OAuth
- `POST /ValidateTokenUseCase` - Validar token de sesión

## Agregar Nuevos Módulos de Negocio

Para agregar su propia lógica de negocio:

1. Cree un nuevo proyecto de módulo (ej: `Module.YourModule.Implement`)
2. Agregue sus casos de uso en `Core.VectorStingerApplication/UserCase/YourModule/`
3. Defina interfaces en `Core.VectorStingerKernel/Interfaces/Managers/YourModule/`
4. Registre el módulo en `Core.VectorStingerApplication/Configurations/VectorStingerMain.cs`

## Licencia

Este proyecto está disponible como base para otros proyectos.

# VectorStingerNet

VectorStingerNet is a Blazor-hosted web application solution (web frontend + host API) implemented with .NET 9 and C# 13. The repository includes a web frontend, a host project, and integration tests that use a distributed application testing harness (`DistributedApplicationTestingBuilder`) to exercise the hosted web frontend.

This README adds the standard sections to make the project easier to understand, build, run, and contribute to.

## Table of Contents
- [Features](#features)
- [Requirements](#requirements)
- [Getting Started](#getting-started)
  - [Clone](#clone)
  - [Build](#build)
  - [Run (CLI)](#run-cli)
  - [Run in Visual Studio](#run-in-visual-studio)
- [Testing](#testing)
- [Configuration](#configuration)
- [Contributing](#contributing)
- [License](#license)
- [Contact](#contact)
- [Acknowledgements](#acknowledgements)

## Features
- Blazor web frontend (hosted configuration)
- Host API project (`Projects.VectorStinger_Host`)
- Integration tests under `VectorStinger.Tests` demonstrating end-to-end validation using a distributed test harness
- Uses modern .NET features: .NET 9 and C# 13

## Requirements
- .NET 9 SDK (install from https://dotnet.microsoft.com)
- Visual Studio 2022 or newer for IDE experience (ensure workload for ASP.NET and web development is installed)
- Recommended: Git for source control

## Getting Started

### Clone
Clone the repository: