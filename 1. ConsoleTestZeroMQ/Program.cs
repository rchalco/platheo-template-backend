using ConsoleTestZeroMQ;
using MessagePack;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
using System.Text.Json;

Console.WriteLine("Programa cliente iniciado...conectando con servidor");

using var client = new DealerSocket();
client.Connect("tcp://localhost:5555");

// Create input
ValidateTokenInput validateTokenInput = new ValidateTokenInput
{
    Token = "xxxxxxxxxxxxxxxxxxxxxxx"
};
var inputBytes = MessagePackSerializer.Serialize(validateTokenInput);

// Send request
client.SendMoreFrame(Array.Empty<byte>());        // Empty frame
client.SendMoreFrame("ValidateTokenUseCase");      // Use case name
client.SendFrame(inputBytes);                      // Request data

// Receive response
var emptyFrame = client.ReceiveFrameBytes();
var responseBytes = client.ReceiveFrameBytes();
var response = MessagePackSerializer.Deserialize<dynamic>(responseBytes);

Console.WriteLine($"Response: {JsonConvert.SerializeObject(response)}");
Console.WriteLine("Programa cliente finalizado. Presione una tecla para salir");
Console.ReadKey();