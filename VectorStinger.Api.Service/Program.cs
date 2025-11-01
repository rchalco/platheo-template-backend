using VectorStinger.Application.Configurations;
using VectorStinger.Core.Configurations;
using VectorStinger.Core.Interfaces.Managers.Security;
using VectorStinger.Infrastructure.DataAccess.Implement.SQLServer;
using VectorStinger.Infrastructure.DataAccess.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using VectorStinger.Modules.Security.Mangers;
using VectorStinger.Api.Service.Middleware;
using VectorStinger.Host.ServiceDefaults;
using VectorStinger.Foundation.Abstractions.UserCase;
using System.Collections.Generic;
using System.Net;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.RateLimiting;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Leer IP y puerto desde la configuración
        var ip = builder.Configuration.GetValue<string>("Kestrel:Endpoints:Http:Ip")
                 ?? builder.Configuration.GetValue<string>("AppIp")
                 ?? "0.0.0.0"; // IP por defecto si no está configurada

        var port = builder.Configuration.GetValue<int?>("Kestrel:Endpoints:Http:Port")
                   ?? builder.Configuration.GetValue<int?>("AppPort")
                   ?? 0; // Puerto por defecto si no está configurado

        if (port != 0)
        {
            // Configurar Kestrel para usar el IP y puerto especificados
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Listen(IPAddress.Parse(ip), port, listenOptions =>
                {
                    listenOptions.UseHttps("api.jato.homes.pfx", "api-roomsy");
                });
            });
        }

        // Add services to the container.
        builder.Services.AddAuthorization();

        // Configuración de CORS
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Habilitar la interfaz Swagger UI
        builder.Services.AddSwaggerGen();

        // Registrar DatabaseSettings con Options Pattern
        builder.Services.AddOptions<DatabaseSettings>().Bind(builder.Configuration.GetSection("DatabaseSettings"));
        builder.Services.AddOptions<PaymentBridgeSettings>().Bind(builder.Configuration.GetSection("PaymentBridgeSettings"));
        builder.Services.AddOptions<List<VectorStingerFolder>>().Bind(builder.Configuration.GetSection("Folders"));

        // Obtener instancia de DatabaseSettings
        var databaseSettings = builder.Configuration.GetSection("DatabaseSettings").Get<DatabaseSettings>();
        List<VectorStingerFolder> roomsyFolders = builder.Configuration.GetSection("Folders").Get<List<VectorStingerFolder>>()!;
        builder.Services.RegisterFoldersConfiguration(roomsyFolders);

        List<Type> userCaseTypes = new List<Type>();
        var serviceUserCase = builder.Services.RegisterUserCases(userCaseTypes, databaseSettings!);
        builder.Services.AddMemoryCache();
        builder.Services.AddApplicationInsightsTelemetry();

        builder.Services.AddRateLimiter(options => {
            options.AddFixedWindowLimiter("api", limiter => {
                limiter.Window = TimeSpan.FromMinutes(1);
                limiter.PermitLimit = 100;
            });
        });

        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            // Habilitar Swagger UI solo en el entorno de desarrollo
            app.UseSwagger();
            app.UseSwaggerUI();
            app.MapGet("/", () => Results.Redirect("/swagger"));
        }

        app.UseHttpsRedirection();

        // Aplicar la política de CORS
        app.UseCors("AllowAll");

        //app.UseAuthorization();
        //configurar middleware para manejar la seguridad       
        app.UseMiddleware<SessionTokenValidationMiddleware>();

        foreach (var service in userCaseTypes)
        {
            var handleMethod = service.GetMethod("ExecuteAsync");
            if (handleMethod == null)
            {
                continue;
            }

            using (var scope = app.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var userCaseInstance = serviceProvider.GetRequiredService(service);

                var inputParameterType = handleMethod.GetParameters().FirstOrDefault()?.ParameterType;

                if (inputParameterType == null || !typeof(IUseCaseInput).IsAssignableFrom(inputParameterType))
                {
                    continue;
                }

                // Obtener el tipo de salida (Result<T>)
                var returnType = handleMethod.ReturnType.GetGenericArguments().FirstOrDefault();
                if (returnType == null)
                {
                    continue;
                }

                app.MapPost("/" + service.Name, async (HttpContext httpContext) =>
                {
                    var input = await httpContext.Request.ReadFromJsonAsync(inputParameterType);
                    if (input == null)
                    {
                        return Results.Problem(
                            detail: "Invalid input data",
                            statusCode: 400,
                            type: "validation-error"
                        );
                    }
                    // Invocar el método ExecuteAsync dinámicamente y obtener el resultado
                    var resultTask = (Task)handleMethod!.Invoke(userCaseInstance, new object[] { input })!;

                    // Esperar el resultado de la tarea
                    await resultTask!;

                    // Obtener el resultado del tipo Task<Result<T>>
                    var resultType = resultTask.GetType().GetProperty("Result")?.PropertyType;
                    var resultProperty = resultTask.GetType().GetProperty("Result");
                    var result = resultProperty?.GetValue(resultTask);

                    // Obtener las propiedades de IsSuccess, Errors y Value del resultado
                    var successProperty = resultType?.GetProperty("IsSuccess");
                    var errorsProperty = resultType?.GetProperty("Errors");
                    var valueProperty = resultType?.GetProperty("Value");

                    var isSuccess = (bool)successProperty?.GetValue(result)!;
                    var errors = (isSuccess) ? null : errorsProperty?.GetValue(result);
                    var value = (isSuccess) ? valueProperty?.GetValue(result) : null;

                    return isSuccess
                        ? Results.Ok(value)
                        : Results.BadRequest(errors);

                })
                    .WithName(service.Name)
                    .WithSummary(userCaseInstance.GetType().GetProperty("Description")?.GetValue(userCaseInstance)?.ToString() ?? "Sin descripción")
                    .WithDescription(userCaseInstance.GetType().GetProperty("Summary")?.GetValue(userCaseInstance)?.ToString() ?? "Sin resumen")
                    .Accepts(inputParameterType, "application/json")
                    .Produces(200, returnType, "application/json")
                    .Produces<ProblemDetails>(400, "application/json");
            }
        }
        
        app.Run();
        
    }
}