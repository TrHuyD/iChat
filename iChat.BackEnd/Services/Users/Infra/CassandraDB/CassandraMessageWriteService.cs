﻿using iChat.BackEnd.Models.Helpers.CassandraOptionss;
using iChat.BackEnd.Models.User.MessageRequests;
using iChat.BackEnd.Services.Users.Infra.IdGenerator;
using Microsoft.AspNetCore.Mvc;
using ISession = Cassandra.ISession;
namespace iChat.BackEnd.Services.Users.Infra.CassandraDB
{
    public class CassandraMessageWriteService
    {
        private readonly SnowflakeService idGen;
        private ISession session;
        public CassandraMessageWriteService(CasandraService _cs,SnowflakeService snowflakeService)
        {
            idGen = snowflakeService;
            session=_cs.GetSession();
        }
        /// <summary>
        /// Straight upload message to the database
        /// No validation is done here
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public async Task<bool> UploadMessageAsync(MesageRequest request)
        {
            if (request.SenderId is null || request.messageType is null)
                throw new ArgumentException("SenderId and MessageType are required.");

            var messageId = idGen.GenerateId();
            var timestamp = DateTime.UtcNow;

           

            var query = "INSERT INTO db_user_message.messages " +
                        "(channel_id, message_id, sender_id, message_type, text_content, media_content, timestamp) " +
                        "VALUES (?, ?, ?, ?, ?, ?, ?);";

            var preparedStatement = await session.PrepareAsync(query);
            var boundStatement = preparedStatement.Bind(
                request.ReceiveChannelId,
                messageId,
                request.SenderId,
                (short)request.messageType,
                request.TextContent ?? string.Empty,
                request.MediaContent ?? string.Empty,
                timestamp
            );

            await session.ExecuteAsync(boundStatement);
            return true;
        }
    }
}
