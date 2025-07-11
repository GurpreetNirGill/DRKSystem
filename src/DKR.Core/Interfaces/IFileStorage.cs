namespace DKR.Core.Interfaces;

public interface IFileStorage
{
    Task<string> UploadAsync(Stream fileStream, string fileName, string containerName);
    Task<Stream> DownloadAsync(string fileName, string containerName);
    Task<bool> DeleteAsync(string fileName, string containerName);
    Task<bool> ExistsAsync(string fileName, string containerName);
    Task<IEnumerable<string>> ListFilesAsync(string containerName);
}