    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    namespace iChat.Client.DTOs.Chat
    {
        public class UserMetadataReact : INotifyPropertyChanged
        {
            public string UserId { get; }

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
                        OnPropertyChanged();
                    }
                }
            }
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
                => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            public UserMetadataReact(string userId, string displayName, string avatarUrl)
            {
                UserId = userId;
                _displayName = displayName;
                _avatarUrl = avatarUrl;
            }
        }

    }
