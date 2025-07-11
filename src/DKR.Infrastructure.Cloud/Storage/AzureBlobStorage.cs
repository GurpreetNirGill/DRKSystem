using Azure.Storage.Blobs;
using DKR.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace DKR.Infrastructure.Cloud.Storage;

public class AzureBlobStorage : IFileStorage
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorage(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AzureStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);
        
        return blobClient.Uri.ToString();
    }

    public async Task<Stream> DownloadAsync(string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        
        var response = await blobClient.DownloadStreamingAsync();
        return response.Value.Content;
    }

    public async Task<bool> DeleteAsync(string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        
        var response = await blobClient.DeleteIfExistsAsync();
        return response.Value;
    }

    public async Task<bool> ExistsAsync(string fileName, string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(fileName);
        
        var response = await blobClient.ExistsAsync();
        return response.Value;
    }

    public async Task<IEnumerable<string>> ListFilesAsync(string containerName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var files = new List<string>();
        
        await foreach (var blobItem in containerClient.GetBlobsAsync())
        {
            files.Add(blobItem.Name);
        }
        
        return files;
    }
}