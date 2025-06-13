namespace iChat.BackEnd.Services.Users.Infra.IdGenerator
{
    public class SnowflakeIdDto
    {
        public long Id { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
    public class SnowflakeService
    {
        private static readonly object _lock = new();

        private readonly long _epoch;
        private readonly int _workerId;
        private readonly int _datacenterId;

        private const long MaxWorkerId = 31;
        private const long MaxDatacenterId = 31;
        private const long SequenceMask = 4095;

        private const int WorkerIdShift = 12;
        private const int DatacenterIdShift = 17;
        private const int TimestampLeftShift = 22;

        private long _lastTimestamp = -1L;
        private long _sequence = 0L;

        public SnowflakeService(int workerId, int datacenterId, long epoch = 1742049259000L)
        {
            _epoch = epoch;

            if (workerId > MaxWorkerId || workerId < 0)
                throw new ArgumentException($"Worker ID must be between 0 and {MaxWorkerId}");

            if (datacenterId > MaxDatacenterId || datacenterId < 0)
                throw new ArgumentException($"Datacenter ID must be between 0 and {MaxDatacenterId}");

            _workerId = workerId;
            _datacenterId = datacenterId;
        }

        public SnowflakeIdDto GenerateId()
        {
            lock (_lock)
            {
                var time= GetCurrentTimestamp();
                var timestamp = time.timestamp;
                if (timestamp < _lastTimestamp)
                    throw new InvalidOperationException("Clock moved backwards!");

                if (timestamp == _lastTimestamp)
                {
                    _sequence = (_sequence + 1) & SequenceMask;
                    if (_sequence == 0)
                    {
                        while(timestamp <= _lastTimestamp)
                        {
                            time = GetCurrentTimestamp();
                            timestamp = time.timestamp;
                        }
                    }    
                }
                else
                {
                    _sequence = 0L;
                }

                _lastTimestamp = timestamp;

                long id =
                    ((timestamp - _epoch) << TimestampLeftShift) |
                    ((long)_datacenterId << DatacenterIdShift) |
                    ((long)_workerId << WorkerIdShift) |
                    _sequence;

                return new SnowflakeIdDto
                {
                    Id = id,
                    CreatedAt =time.dateData
                };
            }
        }

        public DateTime GetDateTimeFromId(long snowflakeId)
        {
            long timestamp = (snowflakeId >> TimestampLeftShift) + _epoch;
            return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
        }

        public long GetTimestampFromId(long snowflakeId)
        {
            return (snowflakeId >> TimestampLeftShift) + _epoch;
        }

        private (long timestamp,DateTimeOffset dateData) GetCurrentTimestamp()
        {
            var dateData = DateTimeOffset.UtcNow;
            var timestamp= dateData.ToUnixTimeMilliseconds();
            return(timestamp,dateData);
        }
        //private long WaitNextMillis(long lastTimestamp)
        //{
        //    long timestamp = GetCurrentTimestamp();
        //    while (timestamp <= lastTimestamp)
        //        timestamp = GetCurrentTimestamp();
        //    return timestamp;
        //}
    }


}
