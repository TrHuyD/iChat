using iChat.Client.Services.UserServices.Chat.Util;
using System.ComponentModel;
    using System.Runtime.CompilerServices;

    namespace iChat.Client.DTOs.Chat
    {
        public class UserMetadataReact 
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
                        _onChanged?.Invoke();
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
                        _avatarUrl =URLsanitizer.Apply(value);

                     _onChanged?.Invoke();
                    }
                }
            }
        private Action? _onChanged;
        public void RegisterOnChange(Action callback)
            => _onChanged = callback;

        public UserMetadataReact(long userId, string displayName, string avatarUrl,long version)
            {
                UserId = userId;
                _displayName = displayName;
                _avatarUrl = URLsanitizer.Apply(avatarUrl);
                Version = version;
             

            }
        }

    }
