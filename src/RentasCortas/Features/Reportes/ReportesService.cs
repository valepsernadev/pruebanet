using System.Text.RegularExpressions;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using RentasCortas.Data;

namespace RentasCortas.Features.Reportes;

public partial class ReportesService : IReportesService
{
    private readonly AppDbContext _context;

    public ReportesService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReporteResultDTO> GenerateReportAsync(Guid ownerId, Guid? inmuebleId, DateOnly? startDate, DateOnly? endDate)
    {
        try
        {
            var periodEnd = endDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var periodStart = startDate ?? periodEnd.AddDays(-30);

            var query = _context.Reservas
                .Include(r => r.Inmueble)
                .Include(r => r.Guest)
                .Where(r => r.Inmueble.OwnerId == ownerId
                    && (r.Status == "confirmed" || r.Status == "completed")
                    && r.CheckIn < periodEnd
                    && r.CheckOut > periodStart);

            string? inmuebleTitle = null;

            if (inmuebleId.HasValue)
            {
                var inmueble = await _context.Inmuebles
                    .FirstOrDefaultAsync(i => i.Id == inmuebleId.Value && i.OwnerId == ownerId)
                    ?? throw new KeyNotFoundException($"El inmueble con id {inmuebleId.Value} no existe o no te pertenece");

                inmuebleTitle = inmueble.Title;
                query = query.Where(r => r.InmuebleId == inmuebleId.Value);
            }

            var reservas = await query
                .OrderBy(r => r.Inmueble.Title)
                .ThenBy(r => r.CheckIn)
                .ToListAsync();

            var fileContent = GenerateExcel(reservas);

            var dateStr = periodEnd.ToString("yyyy-MM-dd");
            var fileName = inmuebleTitle != null
                ? $"reporte-{Slugify(inmuebleTitle)}-{dateStr}.xlsx"
                : $"reporte-completo-{dateStr}.xlsx";

            return new ReporteResultDTO
            {
                FileContent = fileContent,
                FileName = fileName
            };
        }
        catch (Exception ex) when (ex is not KeyNotFoundException)
        {
            throw new InvalidOperationException("Error al generar el reporte", ex);
        }
    }

    private static byte[] GenerateExcel(List<Models.Reserva> reservas)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Reservas");

        var headers = new[] { "Inmueble", "Fecha Check-in", "Fecha Check-out", "Precio Pagado", "Nombre Huésped", "Email Huésped", "Teléfono Huésped", "Estado" };
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        for (var row = 0; row < reservas.Count; row++)
        {
            var r = reservas[row];
            var excelRow = row + 2;

            worksheet.Cell(excelRow, 1).Value = r.Inmueble.Title;
            worksheet.Cell(excelRow, 2).Value = r.CheckIn.ToString("yyyy-MM-dd");
            worksheet.Cell(excelRow, 3).Value = r.CheckOut.ToString("yyyy-MM-dd");
            worksheet.Cell(excelRow, 4).Value = r.TotalPrice;
            worksheet.Cell(excelRow, 5).Value = r.Guest.FullName;
            worksheet.Cell(excelRow, 6).Value = r.Guest.Email;
            worksheet.Cell(excelRow, 7).Value = r.Guest.Phone ?? "";
            worksheet.Cell(excelRow, 8).Value = r.Status;
        }

        worksheet.Cell(reservas.Count + 2, 3).Value = "Total:";
        worksheet.Cell(reservas.Count + 2, 3).Style.Font.Bold = true;
        worksheet.Cell(reservas.Count + 2, 4).FormulaA1 = $"SUM(D2:D{reservas.Count + 1})";
        worksheet.Cell(reservas.Count + 2, 4).Style.Font.Bold = true;

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static string Slugify(string text)
    {
        var slug = text.ToLowerInvariant().Trim();
        slug = SlugRegex().Replace(slug, "-");
        slug = MultiDashRegex().Replace(slug, "-");
        return slug.Trim('-');
    }

    [GeneratedRegex(@"[^a-z0-9\-]")]
    private static partial Regex SlugRegex();

    [GeneratedRegex(@"-{2,}")]
    private static partial Regex MultiDashRegex();
}
