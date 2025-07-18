using iChat.DTOs.Collections;
using iChat.DTOs.Users.Messages;

namespace iChat.Client.Pages.Chat
{
    public partial class ChatChannel
    {
        OnlineUserList _userList;
        private void HandleUserList(MemberList list)
        {

            Console.WriteLine(list.serverId.ToString()+" "+ _currentServerId);
            if(list.serverId.ToString()== _currentServerId)
            _userList.LoadUsers(list.online, list.offline);
            StateHasChanged();
        }
        public void HandleUserListchange((stringlong id, bool online) package)
        {
            var online = package.online;
            var id = package.id;
            if(online)
            _userList.SetUserOnline(id);
            else
                _userList.SetUserOffline(id);
        }
    }
}
