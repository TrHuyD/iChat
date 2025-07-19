using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Users.Enum
{
   public static class SignalrClientPath
    {
        //server_all
        public static string ServerProfileChange = "sa_ServerProfileChange";

        //server_focus
        public static string ChannelCreate = "sf_ChannelCreate";
        public static string RecieveMessage = "sf_RecieveMessage";
        public static string MessageEdit = "sf_MessageEdit";
        public static string MessageDelete = "sf_MessageDelete";
        public static string NewComer = "sf_newMember";
        //public channel focus 
        public static string UserTyping = "cf_UserTyping";

        //onlinelist
        public static string NewUserOnline = "ol_NewUserOnline";
        public static string NewUserOffline = "ol_NewUserOffline";
        public static string UserList = "ol_list";
        //Personal
        public static string JoinNewServer = "p_JoinNewServer";
        public static string LeaverServer = "p_LeaverServer";
        public static string UpdateProfile = "p_UpdateProfile";

        
    }
}
