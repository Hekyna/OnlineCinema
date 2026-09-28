using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

namespace OnlineCinema.Services;

public class AzureBlobService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobService(IConfiguration configuration)
    {
        string connectionString =
            configuration["AzureStorage:ConnectionString"]
            ?? throw new InvalidOperationException("Azure Storage connection string is not configured.");

        string containerName =
            configuration["AzureStorage:ContainerName"]
            ?? throw new InvalidOperationException("Azure Storage container name is not configured.");

        _containerClient = new BlobContainerClient(
            connectionString,
            containerName);
    }

    public async Task UploadAsync(
        Stream stream,
        string blobName,
        string contentType)
    {
        await _containerClient.CreateIfNotExistsAsync(
            PublicAccessType.None);

        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions
            {
                HttpHeaders = new BlobHttpHeaders
                {
                    ContentType = contentType
                }
            },
            CancellationToken.None);
    }

    public async Task DeleteAsync(string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName))
            return;

        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync();
    }

    public string GenerateReadSasUrl(
        string blobName,
        int expirationMinutes = 30)
    {
        BlobClient blobClient = _containerClient.GetBlobClient(blobName);

        BlobSasBuilder sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = _containerClient.Name,
            BlobName = blobName,
            Resource = "b",
            ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes)
        };

        sasBuilder.SetPermissions(BlobSasPermissions.Read);

        return blobClient.GenerateSasUri(sasBuilder).ToString();
    }
}