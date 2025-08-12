namespace iChat.BackEnd.Services.Users.Infra.FileServices
{
    public static class ImageUploadProfiles
    {
        public static readonly ImageUploadProfile Avatar = new(
            Folder: "avatars",
            Transform: img => img.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(128, 128),
                Mode = ResizeMode.Crop
            })),
            WebpQuality: 50
        );

        public static readonly ImageUploadProfile GeneralImage = new(
            Folder: "images",
            Transform: img => { },
            WebpQuality: 85
        );

        public static readonly ImageUploadProfile Emoji = new(
            Folder: "emojis",
            Transform: img => img.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(128, 128), 
                Mode = ResizeMode.Crop
            })),
            WebpQuality: 90
        );
    }
}
