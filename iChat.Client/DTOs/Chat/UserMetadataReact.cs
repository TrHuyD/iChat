using iChat.Client.Services.UserServices.Chat.Util;
using iChat.DTOs.Collections;

namespace iChat.Client.DTOs.Chat
    {
        public class UserMetadataReact 
        {
            public UserId userId { get; set; }
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
                userId = new UserId( userId);
                _displayName = displayName;
                _avatarUrl = URLsanitizer.Apply(avatarUrl);
                Version = version;
             

            }
        public UserMetadataReact(UserId userId, string displayName, string avatarUrl, long version)
        {
            this.userId =userId;
            _displayName = displayName;
            _avatarUrl = URLsanitizer.Apply(avatarUrl);
            Version = version;

        }
        public UserMetadataReact()
        {

        }
    }


    }
