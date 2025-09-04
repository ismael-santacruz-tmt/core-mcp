using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

// Load configuration from appsettings.json
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

string? baseUrl = configuration["BaseUrl"];
string? apiKey = configuration["ApiKey"];

if (string.IsNullOrEmpty(baseUrl))
{
    throw new ArgumentNullException(nameof(baseUrl), "BaseUrl is not configured in appsettings.json.");
}

if (string.IsNullOrEmpty(apiKey))
{
    throw new ArgumentNullException(nameof(apiKey), "ApiKey is not configured in appsettings.json.");
}

// Configure HttpClient
var httpClient = new HttpClient
{
    BaseAddress = new Uri(baseUrl!)
};
httpClient.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
httpClient.Timeout = TimeSpan.FromSeconds(30);



Console.WriteLine("HttpClient configured successfully.");

try
{
    // Endpoint de prueba actualizado para verificar la conexión al ERP
    Console.WriteLine("Iniciando solicitud al endpoint 'customer'...");
    Console.WriteLine($"Base URL: {httpClient.BaseAddress}");
    Console.WriteLine($"Headers: {string.Join(", ", httpClient.DefaultRequestHeaders)}");

    // Cambiar el método HTTP de vuelta a GET para la solicitud de prueba
    var response = await httpClient.GetAsync("/customer");

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Conexión al ERP exitosa.");
    }
    else
    {
        Console.WriteLine($"Error al conectar al ERP. Código de estado: {response.StatusCode}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Excepción al conectar al ERP: {ex.Message}");
}

