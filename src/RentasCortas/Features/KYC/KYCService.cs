using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RentasCortas.Common.Exceptions;
using RentasCortas.Common.Notifications;
using RentasCortas.Data;
using RentasCortas.Models;

namespace RentasCortas.Features.KYC;

public class KYCService : IKYCService
{
    private readonly AppDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly INotificationService _notificationService;

    public KYCService(AppDbContext context, HttpClient httpClient, IConfiguration configuration, INotificationService notificationService)
    {
        _context = context;
        _httpClient = httpClient;
        _configuration = configuration;
        _notificationService = notificationService;
    }

    public async Task<KycResponseDTO> ValidateAsync(Guid userId, IFormFile image)
    {
        try
        {
            var apiKey = _configuration["Anthropic:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
                throw new ServiceUnavailableException(
                    "El servicio de validación de identidad no está disponible. Configure la variable Anthropic:ApiKey para habilitarlo.");

            var user = await _context.Usuarios.FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new KeyNotFoundException($"El usuario con id {userId} no existe");

            var base64Image = await ReadImageAsBase64Async(image);
            var mediaType = image.ContentType;

            var extractedData = await CallClaudeVisionAsync(apiKey, base64Image, mediaType);

            var verdict = DetermineVerdict(extractedData);

            var kycValidation = new KycValidation
            {
                UserId = userId,
                ExtractedName = extractedData.Name,
                ExtractedLastname = extractedData.Lastname,
                ExtractedDocumentNumber = extractedData.DocumentNumber,
                ExtractedBirthdate = extractedData.Birthdate,
                Verdict = verdict,
                ProcessedAt = DateTime.UtcNow
            };

            _context.KycValidations.Add(kycValidation);

            user.KycStatus = verdict;
            await _context.SaveChangesAsync();

            var notificationMessage = verdict == "approved"
                ? "Tu identidad ha sido verificada exitosamente."
                : "La verificación de tu identidad fue rechazada. Por favor, intenta nuevamente con una imagen más clara.";

            await _notificationService.SendInAppAsync(userId, "kyc_validation", notificationMessage);

            return MapToDTO(kycValidation);
        }
        catch (Exception ex) when (ex is not ServiceUnavailableException and not KeyNotFoundException)
        {
            throw new InvalidOperationException("Error al procesar la validación KYC", ex);
        }
    }

    private static async Task<string> ReadImageAsBase64Async(IFormFile image)
    {
        using var memoryStream = new MemoryStream();
        await image.CopyToAsync(memoryStream);
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    private async Task<ExtractedDocumentData> CallClaudeVisionAsync(string apiKey, string base64Image, string mediaType)
    {
        var requestBody = new
        {
            model = "claude-opus-4-6",
            max_tokens = 1024,
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new
                        {
                            type = "image",
                            source = new
                            {
                                type = "base64",
                                media_type = mediaType,
                                data = base64Image
                            }
                        },
                        new
                        {
                            type = "text",
                            text = "Analiza esta imagen de un documento de identidad y extrae la siguiente información. " +
                                   "Responde ÚNICAMENTE con un JSON válido, sin texto adicional, con esta estructura exacta: " +
                                   "{\"name\": \"nombres\", \"lastname\": \"apellidos\", \"document_number\": \"número de documento\", \"birthdate\": \"YYYY-MM-DD\"}. " +
                                   "Si no puedes extraer algún campo, usa null para ese campo."
                        }
                    }
                }
            }
        };

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        var jsonContent = JsonSerializer.Serialize(requestBody, jsonOptions);

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException($"Error en la API de Claude: {response.StatusCode}");

        return ParseClaudeResponse(responseBody);
    }

    private static ExtractedDocumentData ParseClaudeResponse(string responseBody)
    {
        using var document = JsonDocument.Parse(responseBody);
        var content = document.RootElement.GetProperty("content");
        var textBlock = content.EnumerateArray().First(c => c.GetProperty("type").GetString() == "text");
        var text = textBlock.GetProperty("text").GetString() ?? "";

        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');

        if (jsonStart < 0 || jsonEnd < 0)
            return new ExtractedDocumentData();

        var jsonText = text[jsonStart..(jsonEnd + 1)];

        try
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
            return JsonSerializer.Deserialize<ExtractedDocumentData>(jsonText, options) ?? new ExtractedDocumentData();
        }
        catch
        {
            return new ExtractedDocumentData();
        }
    }

    private static string DetermineVerdict(ExtractedDocumentData data)
    {
        var hasAllFields = !string.IsNullOrEmpty(data.Name)
            && !string.IsNullOrEmpty(data.Lastname)
            && !string.IsNullOrEmpty(data.DocumentNumber)
            && data.Birthdate.HasValue;

        return hasAllFields ? "approved" : "rejected";
    }

    private static KycResponseDTO MapToDTO(KycValidation validation)
    {
        return new KycResponseDTO
        {
            Id = validation.Id,
            ExtractedName = validation.ExtractedName,
            ExtractedLastname = validation.ExtractedLastname,
            ExtractedDocumentNumber = validation.ExtractedDocumentNumber,
            ExtractedBirthdate = validation.ExtractedBirthdate,
            Verdict = validation.Verdict ?? "rejected",
            ProcessedAt = validation.ProcessedAt
        };
    }

    private class ExtractedDocumentData
    {
        public string? Name { get; set; }
        public string? Lastname { get; set; }
        public string? DocumentNumber { get; set; }
        public DateOnly? Birthdate { get; set; }
    }
}