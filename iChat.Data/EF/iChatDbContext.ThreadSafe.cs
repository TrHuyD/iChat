using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.Data.EF
{
    public partial class iChatDbContext
    {
        public async Task SaveChangeAsyncSafe()
            {
            try
            {
                await SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency exceptions, e.g., log the error or retry
                Console.WriteLine($"Concurrency error: {ex.Message}");
            }
            catch (DbUpdateException ex)
            {
                // Handle other database update exceptions
                Console.WriteLine($"Database update error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }
    }
}
