namespace GameHubStore.Helpers
{
    public static class FileUploadHelper
    {
        public static async Task<string?> UploadFileAsync(IFormFile? file, string folderName, IWebHostEnvironment environment)
        {
            if (file == null || file.Length == 0)
                return null;

            var uploadsFolder = Path.Combine(environment.WebRootPath, "uploads", folderName);

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using var fileStream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fileStream);

            return $"/uploads/{folderName}/{uniqueFileName}";
        }

        public static void DeleteFile(string? fileUrl, IWebHostEnvironment environment)
        {
            if (string.IsNullOrWhiteSpace(fileUrl))
                return;

            var filePath = Path.Combine(environment.WebRootPath, fileUrl.TrimStart('/'));

            if (File.Exists(filePath))
                File.Delete(filePath);
        }
    }
}