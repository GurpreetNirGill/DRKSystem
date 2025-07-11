using DKR.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DKR.Infrastructure.OnPremise.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly string _basePath;

    public LocalFileStorage(IConfiguration configuration)
    {
        _basePath = configuration["Storage:LocalPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Storage");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string containerName)
    {
        var containerPath = Path.Combine(_basePath, containerName);
        Directory.CreateDirectory(containerPath);
        
        var filePath = Path.Combine(containerPath, fileName);
        
        using (var fileOutputStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileOutputStream);
        }
        
        return filePath;
    }

    public async Task<Stream> DownloadAsync(string fileName, string containerName)
    {
        var filePath = Path.Combine(_basePath, containerName, fileName);
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"File not found: {fileName}");
        }
        
        var memoryStream = new MemoryStream();
        using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            await fileStream.CopyToAsync(memoryStream);
        }
        memoryStream.Position = 0;
        
        return memoryStream;
    }

    public Task<bool> DeleteAsync(string fileName, string containerName)
    {
        var filePath = Path.Combine(_basePath, containerName, fileName);
        
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            return Task.FromResult(true);
        }
        
        return Task.FromResult(false);
    }

    public Task<bool> ExistsAsync(string fileName, string containerName)
    {
        var filePath = Path.Combine(_basePath, containerName, fileName);
        return Task.FromResult(File.Exists(filePath));
    }

    public Task<IEnumerable<string>> ListFilesAsync(string containerName)
    {
        var containerPath = Path.Combine(_basePath, containerName);
        
        if (!Directory.Exists(containerPath))
        {
            return Task.FromResult(Enumerable.Empty<string>());
        }
        
        var files = Directory.GetFiles(containerPath)
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>();
        
        return Task.FromResult(files);
    }
}