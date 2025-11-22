# ? Proyecto VectorStinger.Infrastructure.Bucket - COMPLETADO

## ?? **Proyecto Creado Exitosamente**

Se ha creado el proyecto `VectorStinger.Infrastructure.Bucket` con integración completa de AWS S3.

---

## ?? **Estructura del Proyecto**

```
VectorStinger.Infrastructure.Bucket/
??? Configuration/
?   ??? AwsS3Settings.cs                    ? Configuración de AWS
??? Interfaces/
?   ??? IS3Service.cs                       ? Interface del servicio
??? Models/
?   ??? S3UploadResult.cs                   ? Modelo de resultado
??? Services/
?   ??? AwsS3Service.cs                     ? Implementación completa
??? Extensions/
    ??? ServiceCollectionExtensions.cs      ? Extensiones para DI
```

---

## ?? **Paquetes NuGet Instalados**

| Paquete | Versión | Propósito |
|---------|---------|-----------|
| AWSSDK.S3 | 3.7.400 | AWS S3 SDK |
| AWSSDK.Core | 3.7.400 | AWS Core SDK |
| FluentResults | 3.16.0 | Manejo de resultados |
| Microsoft.Extensions.Logging | 9.0.0 | Logging |
| Microsoft.Extensions.Options | 9.0.0 | Options pattern |
| Microsoft.Extensions.Configuration.Abstractions | 9.0.0 | Configuración |
| Microsoft.Extensions.DependencyInjection.Abstractions | 9.0.0 | DI |
| Microsoft.Extensions.Options.ConfigurationExtensions | 9.0.0 | Options binding |

---

## ? **Funcionalidades Implementadas**

### **1. AwsS3Service**
- ? `UploadFileAsync()` - Subir archivos a S3
- ? `GetPresignedUrlAsync()` - Generar URLs temporales
- ? `DeleteFileAsync()` - Eliminar archivos
- ? `FileExistsAsync()` - Verificar existencia
- ? `GetFileMetadataAsync()` - Obtener metadata

### **2. Características**
- ? Sanitización de nombres de archivo
- ? Generación automática de keys únicos con timestamp
- ? Soporte para metadata personalizada
- ? Manejo robusto de errores
- ? Logging detallado
- ? Dispose pattern implementation
- ? Validación de configuración al inicializar

---

## ?? **Configuración Requerida**

### **appsettings.json**
```json
{
  "AWS": {
    "AccessKey": "",
    "SecretKey": "",
    "BucketName": "platheo-templates-stage",
    "Region": "us-east-1",
    "PathPrefix": "templates",
    "PresignedUrlExpirationMinutes": 60
  }
}
```

### **Variables de Entorno en Azure**
```sh
AWS__AccessKey=secretref:aws-access-key
AWS__SecretKey=secretref:aws-secret-key
AWS__BucketName=platheo-templates-stage
AWS__Region=us-east-1
AWS__PathPrefix=templates
```

---

## ?? **Cómo Usar el Servicio**

### **1. Registrar en Program.cs**
```csharp
using VectorStinger.Infrastructure.Bucket.Extensions;

// En Program.cs
builder.Services.AddAwsS3Services(builder.Configuration);
```

### **2. Inyectar y Usar**
```csharp
using VectorStinger.Infrastructure.Bucket.Interfaces;

public class MyService
{
    private readonly IS3Service _s3Service;

    public MyService(IS3Service s3Service)
    {
        _s3Service = s3Service;
    }

    public async Task<Result<S3UploadResult>> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string contentType)
    {
        var metadata = new Dictionary<string, string>
        {
            ["uploaded-by"] = "user-123",
            ["template-name"] = "my-template"
        };

        return await _s3Service.UploadFileAsync(
            fileStream, 
            fileName, 
            contentType, 
            metadata);
    }
}
```

---

## ?? **Próximos Pasos**

### **Pendientes para Completar el UseCase:**

1. ? ~~Proyecto VectorStinger.Infrastructure.Bucket creado~~
2. ? **Agregar referencia al proyecto en VectorStinger.Core**
3. ? **Agregar referencia al proyecto en VectorStinger.Application**
4. ? **Crear Input/Output/Validation del UseCase**
5. ? **Crear DTOs para Manager**
6. ? **Implementar lógica en IWebTemplateManager**
7. ? **Crear el UseCase completo**
8. ? **Actualizar appsettings.json**
9. ? **Configurar secrets en Azure**

---

## ?? **Comandos para Agregar Referencias**

```sh
# Agregar referencia a VectorStinger.Core
dotnet add VectorStinger.Core reference VectorStinger.Infrastructure.Bucket

# Agregar referencia a VectorStinger.Application
dotnet add VectorStinger.Application reference VectorStinger.Infrastructure.Bucket

# Agregar referencia a VectorStinger.Api.Service
dotnet add VectorStinger.Api.Service reference VectorStinger.Infrastructure.Bucket
```

---

## ?? **Ejemplo de Uso Completo**

```csharp
// Subir archivo
var uploadResult = await _s3Service.UploadFileAsync(
    fileStream,
    "template.zip",
    "application/zip",
    new Dictionary<string, string>
    {
        ["user-id"] = "123",
        ["template-id"] = "456"
    });

if (uploadResult.IsSuccess)
{
    var result = uploadResult.Value;
    Console.WriteLine($"Archivo subido: {result.FileUrl}");
    Console.WriteLine($"Key: {result.Key}");
    Console.WriteLine($"Tamaño: {result.FileSize} bytes");
    
    // Generar URL presignada para acceso temporal
    var urlResult = await _s3Service.GetPresignedUrlAsync(result.Key, 60);
    if (urlResult.IsSuccess)
    {
        Console.WriteLine($"URL temporal: {urlResult.Value}");
    }
}
else
{
    Console.WriteLine($"Error: {string.Join(", ", uploadResult.Errors)}");
}
```

---

## ?? **Seguridad**

### **Configuración de Secrets en Azure Container Apps**

```sh
# 1. Crear secrets
az containerapp secret set \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --secrets \
    "aws-access-key=TU_AWS_ACCESS_KEY" \
    "aws-secret-key=TU_AWS_SECRET_KEY"

# 2. Configurar variables de entorno
az containerapp update \
  --name ca-platheotemplate-stg \
  --resource-group Platheo-tempalte \
  --set-env-vars \
    "AWS__AccessKey=secretref:aws-access-key" \
    "AWS__SecretKey=secretref:aws-secret-key" \
    "AWS__BucketName=platheo-templates-stage" \
    "AWS__Region=us-east-1"
```

---

## ?? **Consideraciones Importantes**

### **1. Permisos de IAM en AWS**

El usuario/rol de AWS necesita estos permisos:
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Action": [
        "s3:PutObject",
        "s3:GetObject",
        "s3:DeleteObject",
        "s3:GetObjectMetadata",
        "s3:ListBucket"
      ],
      "Resource": [
        "arn:aws:s3:::platheo-templates-stage/*",
        "arn:aws:s3:::platheo-templates-stage"
      ]
    }
  ]
}
```

### **2. CORS en S3 (si se accede desde navegador)**

```json
[
  {
    "AllowedHeaders": ["*"],
    "AllowedMethods": ["GET", "PUT", "POST", "DELETE"],
    "AllowedOrigins": ["https://www.platheo.com"],
    "ExposeHeaders": ["ETag"],
    "MaxAgeSeconds": 3000
  }
]
```

### **3. Lifecycle Policy (opcional - para limpieza automática)**

```json
{
  "Rules": [
    {
      "Id": "DeleteOldFiles",
      "Status": "Enabled",
      "Prefix": "templates/",
      "Expiration": {
        "Days": 365
      }
    }
  ]
}
```

---

## ?? **Estado del Proyecto**

| Componente | Estado |
|------------|--------|
| Proyecto creado | ? Completado |
| Paquetes NuGet | ? Instalados |
| Configuración | ? Implementada |
| Interface IS3Service | ? Creada |
| AwsS3Service | ? Implementado |
| Extensions para DI | ? Creadas |
| Modelos | ? Creados |
| Compilación | ? Sin errores |
| Referencias agregadas | ? Pendiente |
| UseCase completo | ? Pendiente |

---

## ?? **Siguiente Acción**

**¿Quieres que continúe con:**
1. Agregar las referencias del proyecto
2. Crear el Input/Output/Validation del UseCase
3. Implementar el Manager
4. Crear el UseCase completo

**O prefieres que genere un documento completo con todo el código restante para que lo copies manualmente?**

---

**Fecha de creación**: 2025-01-13
**Estado**: ? Proyecto base completado - Listo para integrar
