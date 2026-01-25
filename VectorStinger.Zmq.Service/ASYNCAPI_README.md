# VectorStinger ZeroMQ Service - AsyncAPI Documentation

## Overview

This service exposes VectorStinger use cases through ZeroMQ endpoints using the Router-Dealer pattern. Messages are serialized using MessagePack for efficient binary communication.

## Architecture

```
Client (Dealer) <---> ZeroMQ Router <---> Worker Pool <---> Use Cases
                     (tcp://*:5555)
```

## Connection

- **Protocol**: ZeroMQ Router Socket
- **Default Address**: `tcp://*:5555`
- **Serialization**: MessagePack
- **Pattern**: Request-Reply with multi-worker support

## Message Format

### Request Format

Each request consists of 4 frames:
1. **Client ID** (automatically added by ZeroMQ)
2. **Empty frame** (delimiter)
3. **Use Case Name** (string) - e.g., "ValidateTokenUseCase"
4. **Request Data** (MessagePack serialized input)

### Response Format

Each response consists of 3 frames:
1. **Client ID** (echo back)
2. **Empty frame** (delimiter)
3. **Response Data** (MessagePack serialized output)

Response structure:
```json
{
  "IsSuccess": true,
  "Value": { /* Use case output */ },
  "Errors": null
}
```

## Available Use Cases

### ValidateTokenUseCase

Validates a security token.

**Input:**
```json
{
  "Token": "string"
}
```

**Output:**
```json
{
  "IsValid": true,
  "Token": "string",
  "TimeExpired": "2025-12-29T00:00:00Z"
}
```

### VerifyCredentialOAuthUseCase

Verifies OAuth credentials from a provider.

**Input:**
```json
{
  "Provider": 0,
  "Token": "string"
}
```

**Output:**
```json
{
  "IsValid": true,
  "IdUser": 123,
  "IdSession": 456,
  "Token": "string",
  "Expiration": "2025-12-29T00:00:00Z",
  "Message": "string",
  "NamePerson": "string",
  "PictureUrl": "string"
}
```

### GetQuestionsQuizUseCase

Retrieves all quiz questions.

**Input:**
```json
{}
```

**Output:**
```json
{
  "Questions": [
    {
      "QuestionId": 1,
      "QuestionText": "string",
      "QuestionType": "string",
      "IsClosed": 1,
      "IsActive": 1,
      "Options": [
        {
          "OptionId": 1,
          "OptionText": "string"
        }
      ]
    }
  ]
}
```

### RegisterQuestionsQuizUseCase

Registers quiz answers for a user.

**Input:**
```json
{
  "UserId": 123,
  "Answers": [
    {
      "QuestionId": 1,
      "QuestionType": "string",
      "AnswerText": "string",
      "OptionId": 1
    }
  ]
}
```

**Output:**
```json
{
  "IsSuccess": true,
  "Message": "string",
  "TotalAnswersRegistered": 5
}
```

## Client Example (C#)

```csharp
using NetMQ;
using NetMQ.Sockets;
using MessagePack;

using var client = new DealerSocket();
client.Connect("tcp://localhost:5555");

// Create input
var input = new { Token = "my-token" };
var inputBytes = MessagePackSerializer.Serialize(input);

// Send request
client.SendMoreFrame(Array.Empty<byte>());        // Empty frame
client.SendMoreFrame("ValidateTokenUseCase");      // Use case name
client.SendFrame(inputBytes);                      // Request data

// Receive response
var emptyFrame = client.ReceiveFrameBytes();
var responseBytes = client.ReceiveFrameBytes();
var response = MessagePackSerializer.Deserialize<dynamic>(responseBytes);

if (response.IsSuccess)
{
    Console.WriteLine($"Token is valid: {response.Value.IsValid}");
}
```

## Configuration

Configure the service in `appsettings.json`:

```json
{
  "ZmqSettings": {
    "BindAddress": "tcp://*:5555",
    "WorkerThreads": 4
  }
}
```

- **BindAddress**: ZeroMQ bind address
- **WorkerThreads**: Number of worker threads to process requests (default: 4)

## Error Handling

Errors are returned in the response:

```json
{
  "IsSuccess": false,
  "Error": "Error message",
  "Value": null
}
```

## AsyncAPI Specification

> **Note**: AsyncAPI 3.0 does not have native support for ZeroMQ protocol. This service uses a custom ZeroMQ implementation. For API testing and integration, use the examples provided above.

## Performance Characteristics

- **Concurrency**: Multi-threaded worker pool for parallel request processing
- **Serialization**: MessagePack for compact binary format (~30% smaller than JSON)
- **Pattern**: Request-Reply with automatic load balancing across workers
- **Latency**: Low latency (~1-5ms for simple operations, excluding business logic)

## Monitoring

The service integrates with OpenTelemetry for observability:
- Traces for request processing
- Metrics for throughput and latency
- Logs for debugging and auditing

## Security Considerations

1. **Network Security**: Use `tcp://0.0.0.0:5555` for internal networks only
2. **Authentication**: Implement authentication at the use case level
3. **Encryption**: For production, use ZeroMQ with CurveZMQ for encryption
4. **Input Validation**: All inputs are validated by use case validators

## Future Enhancements

- [ ] Add CurveZMQ encryption support
- [ ] Implement AsyncAPI 3.0 specification when ZeroMQ support is added
- [ ] Add request/response compression
- [ ] Implement message priority queuing
- [ ] Add circuit breaker pattern for resilience
