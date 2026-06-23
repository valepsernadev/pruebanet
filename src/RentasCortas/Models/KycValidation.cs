namespace RentasCortas.Models;

public class KycValidation
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? ExtractedName { get; set; }
    public string? ExtractedLastname { get; set; }
    public string? ExtractedDocumentNumber { get; set; }
    public DateOnly? ExtractedBirthdate { get; set; }
    public string? Verdict { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Usuario User { get; set; } = null!;
}