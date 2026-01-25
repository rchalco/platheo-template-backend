# VectorStinger ZeroMQ Service - Testing Guide

## Quick Test

To test the ZeroMQ service, follow these steps:

### 1. Start the Service

```bash
cd VectorStinger.Zmq.Service
dotnet run
```

You should see output like:
```
info: Program[0]
      Starting ZeroMQ service on tcp://*:5555 with 4 worker threads
info: Program[0]
      ZeroMQ service is listening on tcp://*:5555
info: Program[0]
      Registered 5 use cases
```

### 2. Test with a Simple Client

Create a test file `test-client.csx`:

```csharp
#r "nuget: NetMQ, 4.0.2.2"
#r "nuget: MessagePack, 3.1.4"

using NetMQ;
using NetMQ.Sockets;
using MessagePack;
using System;

Console.WriteLine("Connecting to ZeroMQ service...");

using var client = new DealerSocket();
client.Connect("tcp://localhost:5555");

// Test ValidateTokenUseCase
var input = new { Token = "test-token-12345" };
var inputBytes = MessagePackSerializer.Serialize(input);

Console.WriteLine("Sending request to ValidateTokenUseCase...");

// Send request (3 frames: empty, use case name, data)
client.SendMoreFrame(Array.Empty<byte>());
client.SendMoreFrame("ValidateTokenUseCase");
client.SendFrame(inputBytes);

Console.WriteLine("Waiting for response...");

// Receive response (2 frames: empty, data)
var emptyFrame = client.ReceiveFrameBytes();
var responseBytes = client.ReceiveFrameBytes();

Console.WriteLine($"Received response: {responseBytes.Length} bytes");

// Try to deserialize as dynamic
try 
{
    var response = MessagePackSerializer.Deserialize<Dictionary<string, object>>(responseBytes);
    Console.WriteLine("Response:");
    foreach (var kvp in response)
    {
        Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error deserializing response: {ex.Message}");
    Console.WriteLine($"Raw bytes: {BitConverter.ToString(responseBytes)}");
}

Console.WriteLine("\nTest completed!");
```

Run the test:
```bash
dotnet script test-client.csx
```

### 3. Expected Output

```
Connecting to ZeroMQ service...
Sending request to ValidateTokenUseCase...
Waiting for response...
Received response: XXX bytes
Response:
  IsSuccess: True
  Value: [Object]
  Errors: null

Test completed!
```

### 4. Test Other Use Cases

Modify the test client to test other use cases:

**GetQuestionsQuizUseCase:**
```csharp
var input = new { };  // Empty input
client.SendMoreFrame(Array.Empty<byte>());
client.SendMoreFrame("GetQuestionsQuizUseCase");
client.SendFrame(MessagePackSerializer.Serialize(input));
```

**VerifyCredentialOAuthUseCase:**
```csharp
var input = new { Provider = 0, Token = "oauth-token" };
client.SendMoreFrame(Array.Empty<byte>());
client.SendMoreFrame("VerifyCredentialOAuthUseCase");
client.SendFrame(MessagePackSerializer.Serialize(input));
```

### 5. Monitor Logs

Watch the service logs to see request processing:

```
info: Program[0]
      Worker 0 received request for use case: ValidateTokenUseCase
info: Program[0]
      Worker 0 sent response for use case: ValidateTokenUseCase
```

## Troubleshooting

### Port Already in Use

If you get "Address already in use" error:
```bash
# Find process using port 5555
netstat -ano | findstr :5555
# Kill the process
taskkill /PID <PID> /F
```

### Connection Refused

- Ensure the service is running
- Check firewall settings
- Verify the bind address in appsettings.json

### Invalid Response

- Ensure MessagePack version matches (3.1.4)
- Verify use case name is correct (case-sensitive)
- Check if use case is registered in the service

## Production Deployment

For production deployment:

1. **Configure appsettings.Production.json:**
```json
{
  "ZmqSettings": {
    "BindAddress": "tcp://0.0.0.0:5555",
    "WorkerThreads": 8
  },
  "DatabaseSettings": {
    "DefaultConnection": "secretref:db-connection-string"
  }
}
```

2. **Set environment variables:**
```bash
export ASPNETCORE_ENVIRONMENT=Production
export APPLICATIONINSIGHTS_CONNECTION_STRING="your-app-insights-connection"
```

3. **Run as a service:**
```bash
dotnet publish -c Release
# Then use systemd, Windows Service, or container orchestration
```

## Performance Testing

Use a load testing tool to verify performance:

```bash
# Install k6 or similar load testing tool
# Create load test script
# Run load test with multiple concurrent connections
```

Expected performance:
- Throughput: 10,000+ requests/second
- Latency: <5ms (excluding business logic)
- Concurrent connections: 1000+

## Next Steps

1. ✅ Service is running
2. ✅ Basic test passes
3. ⏳ Load testing
4. ⏳ Integration with client applications
5. ⏳ Production deployment
