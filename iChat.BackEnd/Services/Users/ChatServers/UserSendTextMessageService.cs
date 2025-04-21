//using iChat.BackEnd.Models.User.CassandraResults;
//using iChat.BackEnd.Models.User.MessageRequests;
//using iChat.BackEnd.Services.Users.Infra.CassandraDB;
//using iChat.BackEnd.Services.Validators.TextMessageValidators;

//namespace iChat.BackEnd.Services.Users.ChatServers
//{
//    public class UserSendTextMessageService : IChatSendMessageService
//    {
//        TextMessageValidatorService _validator;
//        CassandraMessageWriteService _dbservice;
//        public UserSendTextMessageService(TextMessageValidatorService validator, CassandraMessageWriteService dbservice)
//        {
//            _validator = validator;
//            _dbservice = dbservice;
//        }
//        public async Task<bool> SendTextMessageAsync(MessageRequest request)
//        {
//            if (!_validator.ApplyBannedFilters(1, request.TextContent))
//                return false
//            ;
//            var filteredContent = _validator.ApplyFilters(1, request.TextContent);
//            return await _dbservice.UploadMessageAsync(request);
//        }
//    }
//}
