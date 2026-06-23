namespace RentasCortas.Common.Storage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subfolder);
    void DeleteFile(string relativePath);
}
