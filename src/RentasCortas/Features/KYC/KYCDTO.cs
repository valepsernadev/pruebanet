namespace RentasCortas.Features.KYC;

public class KycResponseDTO
{
    public Guid Id { get; set; }
    public string? ExtractedName { get; set; }
    public string? ExtractedLastname { get; set; }
    public string? ExtractedDocumentNumber { get; set; }
    public DateOnly? ExtractedBirthdate { get; set; }
    public string Verdict { get; set; } = string.Empty;
    public DateTime? ProcessedAt { get; set; }
}