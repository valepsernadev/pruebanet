using System.Text;
using System.Text.Json;

namespace RentasCortas.Common.Notifications;

public class EmailNotificationService
{
    private const string ResendApiUrl = "https://api.resend.com/emails";
    private const string FromAddress = "RentasCortas <onboarding@resend.dev>";

    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly IHttpClientFactory _httpClientFactory;

    public EmailNotificationService(
        IConfiguration configuration,
        ILogger<EmailNotificationService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _logger = logger;
        _httpClientFactory = httpClientFactory;
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        var apiKey = _configuration["Resend:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            _logger.LogWarning(
                "Resend API Key no configurada. No se enviará el email a {ToEmail} con asunto: {Subject}",
                toEmail, subject);
            return;
        }

        var payload = new
        {
            from = FromAddress,
            to = new[] { toEmail },
            subject,
            html = body
        };

        var jsonContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        try
        {
            _logger.LogInformation("Enviando email a {ToEmail} via Resend API", toEmail);

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var response = await client.PostAsync(ResendApiUrl, jsonContent);
            var responseBody = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Email enviado exitosamente a {ToEmail}. Respuesta: {Response}",
                    toEmail, responseBody);
            }
            else
            {
                _logger.LogError(
                    "Error al enviar email a {ToEmail}. Status: {StatusCode}, Respuesta: {Response}",
                    toEmail, (int)response.StatusCode, responseBody);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error de conexión al enviar email a {ToEmail} via Resend: {Message}",
                toEmail, ex.Message);
        }
    }
}