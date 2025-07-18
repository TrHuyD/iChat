namespace iChat.BackEnd.Collections
{
    public class SkipListSortedSetV2<T> where T : IComparable<T>
    {
        private class Node
        {
            public T Value;
            public Node[] Forwards;
            public int[] Spans; // how many nodes we skip on each level

            public Node(T value, int level)
            {
                Value = value;
                Forwards = new Node[level];
                Spans = new int[level];
            }
        }

        private readonly int _maxLevel;
        private readonly Random _rand;
        private readonly Node _head;
        private int _level;
        private int _count;
        private readonly ReaderWriterLockSlim _lock = new();

        public SkipListSortedSetV2(int maxLevel = 16)
        {
            _maxLevel = maxLevel;
            _rand = new Random();
            _head = new Node(default(T), maxLevel);
            _level = 1;
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try { return _count; }
                finally { _lock.ExitReadLock(); }
            }
        }

        private int RandomLevel()
        {
            int level = 1;
            while (_rand.Next(0, 2) == 1 && level < _maxLevel) level++;
            return level;
        }

        public bool Add(T value, out int index)
        {
            _lock.EnterWriteLock();
            try
            {
                Node[] update = new Node[_maxLevel];
                int[] span = new int[_maxLevel];
                Node x = _head;
                index = 0;

                for (int i = _level - 1; i >= 0; i--)
                {
                    span[i] = i + 1 < _maxLevel ? span[i + 1] : 0;
                    while (x.Forwards[i] != null && x.Forwards[i].Value.CompareTo(value) < 0)
                    {
                        index += x.Spans[i];
                        x = x.Forwards[i];
                    }
                    update[i] = x;
                }

                if (x.Forwards[0] != null && x.Forwards[0].Value.CompareTo(value) == 0)
                {
                    index = -1;
                    return false; // already exists
                }

                int lvl = RandomLevel();
                if (lvl > _level)
                {
                    for (int i = _level; i < lvl; i++)
                    {
                        update[i] = _head;
                        update[i].Spans[i] = _count;
                    }
                    _level = lvl;
                }

                Node newNode = new Node(value, lvl);
                for (int i = 0; i < lvl; i++)
                {
                    newNode.Forwards[i] = update[i].Forwards[i];
                    update[i].Forwards[i] = newNode;

                    newNode.Spans[i] = update[i].Spans[i] - (index - (i > 0 ? span[i - 1] : 0));
                    update[i].Spans[i] = (index - (i > 0 ? span[i - 1] : 0)) + 1;
                }

                for (int i = lvl; i < _level; i++)
                {
                    update[i].Spans[i]++;
                }

                _count++;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Remove(T value, out int oldIndex)
        {
            _lock.EnterWriteLock();
            try
            {
                Node[] update = new Node[_maxLevel];
                Node x = _head;
                oldIndex = 0;

                for (int i = _level - 1; i >= 0; i--)
                {
                    while (x.Forwards[i] != null && x.Forwards[i].Value.CompareTo(value) < 0)
                    {
                        oldIndex += x.Spans[i];
                        x = x.Forwards[i];
                    }
                    update[i] = x;
                }

                x = x.Forwards[0];
                if (x == null || x.Value.CompareTo(value) != 0)
                {
                    oldIndex = -1;
                    return false; // not found
                }

                for (int i = 0; i < _level; i++)
                {
                    if (update[i].Forwards[i] == x)
                    {
                        update[i].Spans[i] += x.Spans[i] - 1;
                        update[i].Forwards[i] = x.Forwards[i];
                    }
                    else
                    {
                        update[i].Spans[i]--;
                    }
                }

                while (_level > 1 && _head.Forwards[_level - 1] == null)
                {
                    _level--;
                }

                _count--;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public T GetAt(int index)
        {
            _lock.EnterReadLock();
            try
            {
                Node x = _head;
                int pos = -1;
                for (int i = _level - 1; i >= 0; i--)
                {
                    while (x.Forwards[i] != null && pos + x.Spans[i] < index)
                    {
                        pos += x.Spans[i];
                        x = x.Forwards[i];
                    }
                }
                return x.Forwards[0].Value;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<T> GetRange(int skip, int take)
        {
            _lock.EnterReadLock();
            try
            {
                var result = new List<T>(take);
                Node x = _head;
                int pos = -1;

                for (int i = _level - 1; i >= 0; i--)
                {
                    while (x.Forwards[i] != null && pos + x.Spans[i] < skip)
                    {
                        pos += x.Spans[i];
                        x = x.Forwards[i];
                    }
                }

                x = x.Forwards[0];
                for (int i = 0; i < take && x != null; i++)
                {
                    result.Add(x.Value);
                    x = x.Forwards[0];
                }

                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        public bool Contains(T value)
        {
            _lock.EnterReadLock();
            try
            {
                Node x = _head;
                for (int i = _level - 1; i >= 0; i--)
                {
                    while (x.Forwards[i] != null && x.Forwards[i].Value.CompareTo(value) < 0)
                    {
                        x = x.Forwards[i];
                    }
                }
                x = x.Forwards[0];
                return x != null && x.Value.CompareTo(value) == 0;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        public void Initialize(IEnumerable<T> items)
        {
            _lock.EnterWriteLock();
            try
            {
                ClearInternal();
                var sortedItems = items.OrderBy(x => x).ToList();
                _count = sortedItems.Count;
                if (_count == 0)
                    return;
                Node[] update = new Node[_maxLevel];
                int[] spanTracker = new int[_maxLevel];
                for (int i = 0; i < _maxLevel; i++)
                {
                    update[i] = _head;
                    _head.Forwards[i] = null;
                    _head.Spans[i] = 0;
                    spanTracker[i] = 0;
                }
                int position = 0;
                foreach (var value in sortedItems)
                {
                    int lvl = RandomLevel();
                    if (lvl > _level)
                    {
                        for (int i = _level; i < lvl; i++)
                        {
                            update[i] = _head;
                            spanTracker[i] = 0;
                        }
                        _level = lvl;
                    }
                    var node = new Node(value, lvl);
                    for (int i = 0; i < lvl; i++)
                    {
                        node.Forwards[i] = update[i].Forwards[i];
                        update[i].Forwards[i] = node;
                        node.Spans[i] = update[i].Spans[i] - spanTracker[i];
                        update[i].Spans[i] = spanTracker[i] + 1;
                        spanTracker[i] = 0;
                        update[i] = node;
                    }
                    for (int i = lvl; i < _level; i++)
                    {
                        update[i].Spans[i]++;
                    }
                    position++;
                    for (int i = 0; i < _level; i++)
                        spanTracker[i]++;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        public List<T> GetAll()
        {
            _lock.EnterReadLock();
            try
            {
                var result = new List<T>(_count);
                var node = _head.Forwards[0];
                while (node != null)
                {
                    result.Add(node.Value);
                    node = node.Forwards[0];
                }
                return result;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }


        private void ClearInternal()
        {
            for (int i = 0; i < _maxLevel; i++)
            {
                _head.Forwards[i] = null;
                _head.Spans[i] = 0;
            }
            _level = 1;
            _count = 0;
        }
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                for (int i = 0; i < _level; i++)
                {
                    _head.Forwards[i] = null;
                    _head.Spans[i] = 0;
                }

                _level = 1;
                _count = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }


    }
}
