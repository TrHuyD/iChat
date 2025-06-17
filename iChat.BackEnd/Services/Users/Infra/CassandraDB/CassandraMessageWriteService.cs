using iChat.BackEnd.Models.Helpers.CassandraOptionss;
using iChat.BackEnd.Models.User.CassandraResults;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using Microsoft.AspNetCore.Mvc;
using ISession = Cassandra.ISession;
namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class CassandraMessageWriteService
    {
      //  private readonly SnowflakeService idGen;
        private ISession session;
        public CassandraMessageWriteService(CasandraService _cs)
        {
          //  idGen = snowflakeService;
            session=_cs.GetSession();
        }
        /// <summary>
        /// Straight upload message to the database
        /// No validation is done here
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<CassMessageWriteResult> UploadMessageAsync(MessageRequest request,SnowflakeIdDto messageId)
        {
            if (request.SenderId is null)
                throw new ArgumentException("SenderId are required.");

            //  var messageId = idGen.GenerateId();
            var offsettimestamp = messageId.CreatedAt; //DateTimeOffset.Now;
            var timestamp = offsettimestamp.DateTime;

           

            var query = "INSERT INTO user_upload.messages " +
                        "(channel_id, message_id, sender_id, message_type, text_content, media_content, timestamp) " +
                        "VALUES (?, ?, ?, ?, ?, ?, ?);";

            var preparedStatement = await session.PrepareAsync(query);
            var boundStatement = preparedStatement.Bind(
                long.Parse(request.ReceiveChannelId),
                messageId.Id,
                long.Parse(request.SenderId),
                (short)request.messageType,
                request.TextContent ?? string.Empty,
                request.MediaContent ?? string.Empty,
                timestamp
            );

            await session.ExecuteAsync(boundStatement);
            return new CassMessageWriteResult { Success = true,CreatedAt  =offsettimestamp};
        }
    }
}
