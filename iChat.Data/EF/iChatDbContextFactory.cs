using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Sqlite;
namespace iChat.Data.EF
{
    class iChatDbContextFactory: IDesignTimeDbContextFactory<iChatDbContext>
    {
        public iChatDbContext CreateDbContext(string[] args)
        {
            // Initialize configuration builder
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets<iChatDbContextFactory>()  
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<iChatDbContext>();

            var connectionString = config["PostgreSQL:ConnectionString"];
            optionsBuilder.UseNpgsql(connectionString);
            optionsBuilder.EnableSensitiveDataLogging();

            return new iChatDbContext(optionsBuilder.Options);
        }

    }
}
