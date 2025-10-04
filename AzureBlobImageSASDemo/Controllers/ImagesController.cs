using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using AzureBlobImageSASDemo.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace AzureBlobImageSASDemo.Controllers
{
    public class ImagesController : Controller
    {
        private readonly string? _connectionString;
        private readonly string? _containerName;

        public ImagesController(IConfiguration configuration)
        {
            // Prefer configuration values (which can come from env vars) but also fallback to Environment
            _connectionString =configuration["AZURE_STORAGE_CONNECTION_STRING"] ?? Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            _containerName = configuration["AZURE_BLOB_CONTAINER_NAME"] ?? Environment.GetEnvironmentVariable("AZURE_BLOB_CONTAINER_NAME");
        }

        public IActionResult Index()
        {
            if (string.IsNullOrWhiteSpace(_connectionString) || string.IsNullOrWhiteSpace(_containerName))
            {
                ViewBag.Error = "Missing configuration. Please set environment variables AZURE_STORAGE_CONNECTION_STRING and AZURE_BLOB_CONTAINER_NAME.";
                return View(new List<ImageViewModel>());
            }

            try
            {
                var images = GetImagesWithSasUrls();
                return View(images);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error connecting to Azure Blob Storage: " + ex.Message;
                return View(new List<ImageViewModel>());
            }
        }

        private List<ImageViewModel> GetImagesWithSasUrls()
        {
            var blobServiceClient = new BlobServiceClient(_connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(_containerName);
            var imageList = new List<ImageViewModel>();

            foreach (var blobItem in containerClient.GetBlobs())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);

                // SAS valid for 5 minutes
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = _containerName,
                    BlobName = blobItem.Name,
                    Resource = "b", // 'b' for blob
                    ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                Uri sasUri;

                // If the client can generate SAS directly (has account key), use it; otherwise construct from storage account key is required.
                if (blobClient.CanGenerateSasUri)
                {
                    sasUri = blobClient.GenerateSasUri(sasBuilder);
                }
                else
                {
                    // Fallback: try to build SAS using the service client and account key (requires connection string with key)
                    throw new InvalidOperationException("BlobClient cannot generate SAS. Ensure connection string includes account key or use a different method to generate SAS.");
                }

                imageList.Add(new ImageViewModel
                {
                    FileName = blobItem.Name,
                    SasUrl = sasUri.ToString()
                });
            }

            return imageList;
        }
    }
}
