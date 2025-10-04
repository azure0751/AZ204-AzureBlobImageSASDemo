# AzureBlobImageSASDemo

ASP.NET Core MVC demo (NET 9.0) that lists images from an Azure Blob Storage container and generates **5-minute SAS URLs** for each image.

## Configuration (environment variables)

- `AZURE_STORAGE_CONNECTION_STRING` - connection string for your storage account (should contain account key to allow SAS generation).
- `AZURE_BLOB_CONTAINER_NAME` - container name that contains image blobs.

Example (Windows PowerShell):
```powershell
$env:AZURE_STORAGE_CONNECTION_STRING = "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net"
$env:AZURE_BLOB_CONTAINER_NAME = "images"
dotnet run
```

## Docker

Build and run with Docker (example):
```bash
docker build -t azureblobimagesasdemo .
docker run -e AZURE_STORAGE_CONNECTION_STRING="<your-conn-str>" -e AZURE_BLOB_CONTAINER_NAME="images" -p 5000:8080 azureblobimagesasdemo
```

## Notes

- The app expects the connection string to contain the account key so the server can generate SAS tokens.
- If running in environments where you cannot store account keys, consider generating user-delegation SAS via Azure AD.
