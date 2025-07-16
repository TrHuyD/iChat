    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    namespace iChat.Client.DTOs.Chat
    {
        public class UserMetadataReact : INotifyPropertyChanged
        {
            public long UserId { get; }
            public long Version { get; set; }
            private string _displayName;
            public string DisplayName
            {
                get => _displayName;
                set
                {
                    if (_displayName != value)
                    {
                        _displayName = value;
                        OnPropertyChanged();
                    }
                }
            }
            private string _avatarUrl;
            public string AvatarUrl
            {
                get => _avatarUrl;
                set
                {
                    if (_avatarUrl != value)
                    {
                        _avatarUrl = value;
                    AvatarUrlSanitizer();
                        OnPropertyChanged();
                    }
                }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        private void AvatarUrlSanitizer()
        {
            if (string.IsNullOrEmpty(_avatarUrl))
            {
                _avatarUrl = "https://cdn.discordapp.com/embed/avatars/0.png";
            }
            else
     if (!_avatarUrl.StartsWith("http"))
            {
#if DEBUG
                _avatarUrl = "https://localhost:6051" + _avatarUrl;
#else
                                        _avatarUrl = "https://ichat.dedyn.io" +_avatarUrl;
#endif
            }
        }
            public UserMetadataReact(long userId, string displayName, string avatarUrl,long version)
            {
                UserId = userId;
                _displayName = displayName;
                _avatarUrl = avatarUrl;
                Version = version;
                AvatarUrlSanitizer();

            }
        }

    }
