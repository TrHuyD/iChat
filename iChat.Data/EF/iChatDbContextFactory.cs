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
            //var config = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json").Build();
       
        var optionsBuilder = new DbContextOptionsBuilder<iChatDbContext>();
            var dbPath = Path.Combine(AppContext.BaseDirectory, "app.db");
            var connectionString = $"Data Source={dbPath};";

            optionsBuilder.UseSqlite(connectionString);

            //optionsBuilder.UseSqlServer(config.GetConnectionString("iChatdev"));
            //  optionsBuilder.usesqlli
            optionsBuilder.EnableSensitiveDataLogging();

        return new iChatDbContext(optionsBuilder.Options);
    }

}
}
