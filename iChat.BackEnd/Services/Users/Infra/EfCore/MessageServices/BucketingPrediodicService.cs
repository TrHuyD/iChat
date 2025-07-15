using iChat.BackEnd.Services.UtilServices;
using iChat.Data.EF;
using iChat.DTOs.Users.Messages;
using Microsoft.EntityFrameworkCore;

namespace iChat.BackEnd.Services.Users.Infra.EfCore.MessageServices
{
    public class BucketingPrediodicService : PeriodicService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MessageTimeLogger _logger;
        public BucketingPrediodicService(IServiceScopeFactory scopeFactory,MessageTimeLogger logger)
            : base(TimeSpan.FromMinutes(10))
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
        }

        protected override bool EnableRequirement() => _logger.IsWrittenRecently(TimeSpan.FromMinutes(10));

        protected override async Task ExecuteTask()
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<iChatDbContext>();
            var report= await dbContext.Database.SqlQueryRaw<BucketingReport>("SELECT * FROM process_messages_bucketing();").ToListAsync();
        }
    }
}
