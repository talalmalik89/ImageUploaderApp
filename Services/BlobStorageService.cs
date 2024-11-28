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
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType)
        {
            var blobContainerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            await blobContainerClient.CreateIfNotExistsAsync();


            var blobClient = blobContainerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream);

            string imageUrl = GenerateSasUri(blobClient).ToString();

            await SaveInDb(fileName, imageUrl, contentType);

            return imageUrl;
        }

        private async Task SaveInDb(string fileName, string imageUrl, string contentType)
        {
            var scope = _serviceProvider.CreateScope();
            var _dbContext = scope.ServiceProvider.GetService<AppDbContext>();
            // Save image info in SQL
            var imageData = new ImageData
            {
                FileName = fileName,
                Url = imageUrl,
                UploadDate = DateTime.UtcNow,
                ContentType = contentType
            };

            await _dbContext.FileData.AddAsync(imageData);
            await _dbContext.SaveChangesAsync();
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
