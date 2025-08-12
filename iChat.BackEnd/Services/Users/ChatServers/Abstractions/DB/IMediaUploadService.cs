using iChat.BackEnd.Services.Users.Infra.FileServices;
using iChat.Data.Entities.Users.Messages;
using iChat.DTOs.Collections;
using System.Threading.Tasks;

namespace iChat.BackEnd.Services.Users.ChatServers.Abstractions.DB
{

        public interface IMediaUploadService : IDisposable
        {
            /// <summary>
            /// Saves an image if it doesn't already exist in DB (deduplicated by SHA256 hash).
            /// Returns null if duplicate detected.
            /// The file is staged in memory/disk until CommitAsync() is called.
            /// </summary>
            Task<MediaFile?> SaveImageWithHashDedupAsync(
                IFormFile file,
                UserId uploaderUserId,
                ImageUploadProfile profile);
            /// <summary>
            /// Saves an image using a provided filename (no dedup check).
            /// The file is staged in memory/disk until CommitAsync() is called.
            /// </summary>
            Task<MediaFile> SaveImageWithProvidedIdAsync(
                IFormFile file,
                UserId uploaderUserId,
                ImageUploadProfile profile,
                string fileName);
            /// <summary>
            /// Commits all staged files to the database.
            /// Throws if no upload attempts were made (unless only duplicates were attempted, in which case no-op).
            /// </summary>
            Task CommitAsync();
            /// <summary>
            /// Rolls back all staged files from disk.
            /// Throws if no upload attempts were made (unless only duplicates were attempted, in which case no-op).
            /// </summary>
            Task RollbackAsync();
        }
    }

