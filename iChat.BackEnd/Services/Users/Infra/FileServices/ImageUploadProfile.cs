namespace iChat.BackEnd.Services.Users.Infra.FileServices
{
    public record ImageUploadProfile(
        string Folder,
        Action<Image> Transform,
        int WebpQuality
    );
}
