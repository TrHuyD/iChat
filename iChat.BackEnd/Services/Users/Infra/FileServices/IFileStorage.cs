namespace iChat.BackEnd.Services.Users.Infra.FileServices
{
    public interface IFileStorage
    {
        Task<bool> UploadFileAsync(IFormFile file, string name, string dir = "", Func<Task<bool>>? filter = null);
        Task<bool> UploadFileAsync(Stream fileStream, string name, string dir = "", Func<Task<bool>>? filter = null);
        Task<Stream?> DownloadFileAsync(string filePath);
        Task<bool> DeleteFileAsync(string filePath);
        Task<bool> FileExistsAsync(string filePath);
        Task<List<string>> ListFilesAsync(string dir = "");
        Task<string?> GetFileUrlAsync(string filePath, TimeSpan? expiry = null);
    }

}
