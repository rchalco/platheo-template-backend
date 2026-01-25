# ?? Casos de Uso Registrados en VectorStinger.Zmq.Service

## ?? Resumen

El servicio **VectorStinger.Zmq.Service** levanta automáticamente **5 casos de uso** desde el ensamblado `VectorStinger.Application`.

---

## ?? Lista de Casos de Uso Disponibles

### **Módulo: Security (Seguridad)** ??

#### 1. **ValidateTokenUseCase**
- **Namespace:** `VectorStinger.Application.UserCase.Security.ValidateToken`
- **Propósito:** Validar tokens de sesión
- **Input:** `ValidateTokenInput`
- **Output:** `ValidateTokenOutPut`
- **Validation:** `ValidateTokenValidation`
- **Archivos:**
  - `ValidateTokenUseCase.cs`
  - `ValidateTokenInput.cs`
  - `ValidateTokenOutPut.cs`
  - `ValidateTokenValidation.cs`

#### 2. **VerifyCredentialOAuthUseCase**
- **Namespace:** `VectorStinger.Application.UserCase.Security.VerifyCredentialOAuth`
- **Propósito:** Verificar credenciales OAuth (Firebase)
- **Input:** `VerifyCredentialOAuthInput`
- **Output:** `VerifyCredentialOAuthOutput`
- **Validation:** `VerifyCredentialValidation`
- **Archivos:**
  - `VerifyCredentialOAuthUseCase.cs`
  - `VerifyCredentialOAuthInput.cs`
  - `VerifyCredentialOAuthOutput.cs`
  - `VerifyCredentialValidation.cs`

---

### **Módulo: WebTemplate (Plantillas Web)** ??

#### 3. **RegisterTemplateUseCase**
- **Namespace:** `VectorStinger.Application.UserCase.WebTemplate.RegisterTemplate`
- **Propósito:** Registrar nuevas plantillas/templates
- **Input:** `RegisterTemplateInput`
- **Output:** `RegisterTemplateOutput`
- **Validation:** `RegisterTemplateValidation`
- **Archivos:**
  - `RegisterTemplateUseCase.cs`
  - `RegisterTemplateInput.cs`
  - `RegisterTemplateOutput.cs`
  - `RegisterTemplateValidation.cs`

#### 4. **RegisterQuestionsQuizUseCase**
- **Namespace:** `VectorStinger.Application.UserCase.WebTemplate.RegisterQuestionsQuiz`
- **Propósito:** Registrar preguntas de quiz/cuestionarios
- **Input:** `RegisterQuestionsQuizInput`
- **Output:** `RegisterQuestionsQuizOutput`
- **Validation:** `RegisterQuestionsQuizValidation`
- **Archivos:**
  - `RegisterQuestionsQuizUseCase.cs`
  - `RegisterQuestionsQuizInput.cs`
  - `RegisterQuestionsQuizOutput.cs`
  - `RegisterQuestionsQuizValidation.cs`

#### 5. **GetQuestionsQuizUseCase**
- **Namespace:** `VectorStinger.Application.UserCase.WebTemplate.GetQuestionsQuiz`
- **Propósito:** Obtener preguntas de quiz
- **Input:** `GetQuestionsQuizInput`
- **Output:** `GetQuestionsQuizOutput`
- **Validation:** `GetQuestionsQuizValidation`
- **Archivos:**
  - `GetQuestionsQuizUseCase.cs`
  - `GetQuestionsQuizInput.cs`
  - `GetQuestionsQuizOutput.cs`
  - `GetQuestionsQuizValidation.cs`

---

## ?? Configuración del Servicio ZeroMQ

### **Archivo de Configuración**
El servicio lee la configuración desde `appsettings.json`:

```json
{
  "ZmqSettings": {
    "BindAddress": "tcp://*:5555",
    "WorkerThreads": 4
  }
}
```

### **Comportamiento del Servicio**
- **Puerto por defecto:** 5555
- **Protocolo:** TCP
- **Workers:** 4 hilos (configurable)
- **Patrón:** Router (recibe requests de múltiples clientes)
- **Serialización:** MessagePack

---

## ?? Cómo Funciona el Registro

### **Proceso de Inicio:**

1. **Startup** (`Program.cs`):
   ```csharp
   List<Type> userCaseTypes = new List<Type>();
   builder.Services.RegisterUserCases(userCaseTypes, databaseSettings!, builder.Configuration);
   ```

2. **Registro Automático** (`VectorStingerMain.cs`):
   ```csharp
   // Escanea el ensamblado VectorStinger.Application
   var useCaseTypes = allTypes
       .Where(t => t.IsClass && !t.IsAbstract && typeof(IUseCase).IsAssignableFrom(t))
       .ToList();

   // Registra cada caso de uso encontrado
   foreach (var userCase in useCaseTypes)
   {
       services.AddTransient(userCase);
       serviceUserCase.Add(userCase); // Agrega a la lista
   }
   ```

3. **Disponibilidad en ZeroMQ**:
   - Los casos de uso se almacenan en `_userCaseTypes`
   - El servicio escucha requests con el nombre del caso de uso
   - Busca el tipo correspondiente por nombre de clase

---

## ?? Uso desde Cliente

### **Formato de Request:**

```
Frame 1: Client ID (generado por ZeroMQ)
Frame 2: Empty delimiter
Frame 3: Use Case Name (string) - ej: "ValidateTokenUseCase"
Frame 4: Request Data (MessagePack serialized)
```

### **Ejemplo de Llamada:**

```csharp
using NetMQ;
using NetMQ.Sockets;
using MessagePack;

// Cliente ZeroMQ
using var client = new DealerSocket();
client.Connect("tcp://localhost:5555");

// Preparar input
var input = new ValidateTokenInput
{
    Token = "abc123...",
    SessionId = 456
};

// Serializar con MessagePack
var requestData = MessagePackSerializer.Serialize(input);

// Enviar request
client.SendMoreFrame(string.Empty);           // Empty delimiter
client.SendMoreFrame("ValidateTokenUseCase"); // Use case name
client.SendFrame(requestData);                // Request data

// Recibir response
var responseData = client.ReceiveFrameBytes();
var response = MessagePackSerializer.Deserialize<dynamic>(responseData);

Console.WriteLine($"IsSuccess: {response.IsSuccess}");
Console.WriteLine($"Value: {response.Value}");
```

---

## ?? Arquitectura del Servicio

```
???????????????????????????????????????????????????????????
?              VectorStinger.Zmq.Service                  ?
???????????????????????????????????????????????????????????
?                                                         ?
?  Program.cs                                            ?
?  ?? RegisterUserCases() ? Escanea y registra          ?
?  ?   ?? VectorStinger.Application assembly            ?
?  ?       ?? Security Module (2 use cases)             ?
?  ?       ?? WebTemplate Module (3 use cases)          ?
?  ?                                                     ?
?  ?? ZmqHostedService                                   ?
?      ?? RouterSocket (tcp://*:5555)                    ?
?      ?? Worker Threads (4 por defecto)                ?
?      ?? ProcessRequestAsync()                          ?
?          ?? Busca use case por nombre                 ?
?          ?? Deserializa MessagePack                   ?
?          ?? Ejecuta ExecuteAsync()                     ?
?          ?? Serializa respuesta                        ?
?                                                         ?
???????????????????????????????????????????????????????????
```

---

## ?? Comandos para Listar Casos de Uso

### **Opción 1: Script PowerShell**

Crear archivo `list-usecases.ps1`:
```powershell
# Lista todos los casos de uso registrados
$projectPath = "VectorStinger.Application"
$useCases = Get-ChildItem -Path $projectPath -Recurse -Filter "*UseCase.cs" | 
    Where-Object { $_.Name -ne "BaseUseCase.cs" -and $_.Name -ne "IUseCase.cs" }

Write-Host "==================================" -ForegroundColor Cyan
Write-Host "  Casos de Uso Registrados" -ForegroundColor Cyan
Write-Host "==================================" -ForegroundColor Cyan
Write-Host ""

$count = 0
$useCases | ForEach-Object {
    $count++
    $useCaseName = $_.BaseName
    $relativePath = $_.FullName.Replace((Get-Location).Path + "\", "")
    
    Write-Host "$count. $useCaseName" -ForegroundColor Green
    Write-Host "   Path: $relativePath" -ForegroundColor Gray
    Write-Host ""
}

Write-Host "Total: $count casos de uso" -ForegroundColor Yellow
```

### **Opción 2: Comando .NET**

```bash
# Buscar todos los archivos *UseCase.cs
dotnet build VectorStinger.Application
find VectorStinger.Application/UserCase -name "*UseCase.cs" -type f
```

### **Opción 3: Logging en Tiempo de Ejecución**

Agregar en `ZmqHostedService`:
```csharp
_logger.LogInformation("=== Casos de Uso Registrados ===");
foreach (var (index, useCaseType) in _userCaseTypes.Select((t, i) => (i + 1, t)))
{
    _logger.LogInformation("{Index}. {UseCaseName} ({FullName})", 
        index, useCaseType.Name, useCaseType.FullName);
}
_logger.LogInformation("Total: {Count} casos de uso", _userCaseTypes.Count);
```

---

## ?? Verificación en Logs

Al iniciar el servicio, verás en los logs:

```
info: ZmqHostedService[0]
      Starting ZeroMQ service on tcp://*:5555 with 4 worker threads
info: ZmqHostedService[0]
      ZeroMQ service is listening on tcp://*:5555
info: ZmqHostedService[0]
      Registered 5 use cases
info: ZmqHostedService[0]
      Worker 0 started
info: ZmqHostedService[0]
      Worker 1 started
info: ZmqHostedService[0]
      Worker 2 started
info: ZmqHostedService[0]
      Worker 3 started
```

---

## ?? Resumen Final

| Módulo | Casos de Uso | Descripción |
|--------|--------------|-------------|
| **Security** | 2 | ValidateToken, VerifyCredentialOAuth |
| **WebTemplate** | 3 | RegisterTemplate, RegisterQuestionsQuiz, GetQuestionsQuiz |
| **TOTAL** | **5** | Todos registrados automáticamente |

---

## ?? Referencias

- **Proyecto:** `VectorStinger.Zmq.Service`
- **Ensamblado de Casos de Uso:** `VectorStinger.Application`
- **Patrón:** CQRS con Use Cases
- **Protocolo:** ZeroMQ + MessagePack
- **Puerto:** TCP 5555 (configurable)

