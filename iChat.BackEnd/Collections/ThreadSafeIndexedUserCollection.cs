namespace iChat.BackEnd.Collections
{
    /// <summary>
    /// Specialized version for long user IDs
    /// </summary>
    public class ThreadSafeIndexedUserCollection : ThreadSafeIndexedCollection<long>
    {
        public bool AddUser(long userId) => Add(userId);
        public bool RemoveUser(long userId) => Remove(userId);
        public bool ContainsUser(long userId) => Contains(userId);
        public List<long> GetUserPage(int skip, int take) => GetChunk(skip, take);
        public List<long> GetAllUsers() => ToList();
        public void InitializeUsers(IEnumerable<long> userIds) => Initialize(userIds);
        public void AddUsers(IEnumerable<long> userIds) => AddRange(userIds);
    }
}
