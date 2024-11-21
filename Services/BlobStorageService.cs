using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using ImageUploaderApp.Models;
using ImageUploaderApp.Data;

namespace ImageUploaderApp.Services
{
public class BlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string? _containerName;
    private readonly IServiceProvider _serviceProvider;

    public BlobStorageService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        var connectionString = configuration.GetConnectionString("AzureBlobStorage");
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerName = configuration["AzureBlobStorage:ContainerName"];
        _serviceProvider = serviceProvider;
        //_dbContext = dbContext;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
    {
        var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await blobContainerClient.CreateIfNotExistsAsync();

        var blobClient = blobContainerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream);

        string imageUrl = GenerateSasUri(blobClient).ToString();

        using var scope = _serviceProvider.CreateScope();
           await using var _dbContext = scope.ServiceProvider.GetService<AppDbContext>();
        // Save image info in SQL
        var imageData = new ImageData
        {
            FileName = fileName,
            Url = imageUrl,
            UploadTime = DateTime.UtcNow
        };

        await _dbContext.ImageDatas.AddAsync(imageData);
        await _dbContext.SaveChangesAsync();

        return imageUrl;
    }

    private Uri GenerateSasUri(BlobClient blobClient)
    {
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerName,
            BlobName = blobClient.Name,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(2)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        return blobClient.GenerateSasUri(sasBuilder);
    }
}

}
