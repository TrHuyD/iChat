namespace iChat.BackEnd.Collections
{
    public readonly struct stringlong : IEquatable<stringlong>
    {
        private readonly long _value;
        private readonly string? _stringValue;
        public long Value => _value;
        public string StringValue => _stringValue ?? _value.ToString();

        // Constructor from long
        public stringlong(long value)
        {
            _value = value;
            _stringValue = null;
        }

        // Constructor from string
        public stringlong(string value)
        {
            _value = long.Parse(value);
            _stringValue = value;
        }

        public override string ToString() => StringValue;

        // Implicit conversions
        public static implicit operator long(stringlong id) => id._value;
        public static implicit operator stringlong(long value) => new(value);
        public static implicit operator stringlong(string value) => new(value);

        // Explicit to string
        public static explicit operator string(stringlong id) => id.StringValue;

        // Equality
        public bool Equals(stringlong other) => _value == other._value;
        public override bool Equals(object? obj) => obj is stringlong other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
    }

}
