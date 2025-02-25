
using iChat.Data.Entities.Users.Messages;
using iChat.ViewModels.Users.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.Configurations
{
    public class MessageConfiguration : IEntityTypeConfiguration<Message>
    {
        public void Configure(EntityTypeBuilder<Message> builder)
        {
            builder.Property(x => x.Timestamp).IsRequired();
            builder.HasDiscriminator<MessageType>(x => x.MessageType)
                .HasValue<MediaMessage>(MessageType.Media)
                .HasValue<TextMessage>(MessageType.Text);
            builder.HasKey(m => m.MessageId);
            builder.HasOne(m => m.Sender)
               .WithMany()
               .HasForeignKey(m => m.SenderId)
               .OnDelete(DeleteBehavior.Restrict);
            builder.HasOne(m => m.Reciever)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
