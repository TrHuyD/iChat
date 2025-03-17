namespace iChat.BackEnd.Services.Users.Infra.IdGenerator
{
public class SnowflakeService
{
    private static readonly object _lock = new();
                                   
    private readonly long _epoch ;
    private readonly int _workerId;
    private readonly int _datacenterId;
    private readonly long _maxWorkerId = 31;
    private readonly long _maxDatacenterId = 31;
    private readonly long _sequenceMask = 4095;
    private readonly int _workerIdShift = 12;
    private readonly int _datacenterIdShift = 17;
    private readonly int _timestampLeftShift = 22;


        private long _lastTimestamp = -1L;
    private long _sequence = 0L;

    public SnowflakeService(int workerId, int datacenterId,long epoch= 1742049259000L)
        {
        _epoch=epoch;
            if (workerId > _maxWorkerId || workerId < 0)
            throw new ArgumentException($"Worker ID must be between 0 and {_maxWorkerId}");
        
        if (datacenterId > _maxDatacenterId || datacenterId < 0)
            throw new ArgumentException($"Datacenter ID must be between 0 and {_maxDatacenterId}");

        _workerId = workerId;
        _datacenterId = datacenterId;
    }
    //1 unused
    //41 timestamp in mili
    //10 workerid(5 workers + 5 datacenter)
    //12 Sequence number (auto increase per mili)
    public long GenerateId()
    {
        lock (_lock)
        {
            long timestamp = GetCurrentTimestamp();

            if (timestamp < _lastTimestamp)
                throw new InvalidOperationException("Clock moved backwards!");

            if (timestamp == _lastTimestamp)
            {
                _sequence = _sequence + 1 & _sequenceMask;
                if (_sequence == 0) timestamp = WaitNextMillis(_lastTimestamp);
            }
            else _sequence = 0L;

            _lastTimestamp = timestamp;

            return timestamp - _epoch << _timestampLeftShift |
                   _datacenterId << _datacenterIdShift |
                   _workerId << _workerIdShift |
                   _sequence;
        }
    }

    private long GetCurrentTimestamp() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    private long WaitNextMillis(long lastTimestamp)
    {
        long timestamp = GetCurrentTimestamp();
        while (timestamp <= lastTimestamp) timestamp = GetCurrentTimestamp();
        return timestamp;
    }
}

}
