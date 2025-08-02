using System.Text.Json;
using System.Text.Json.Serialization;

namespace iChat.DTOs.Collections
{
    [JsonConverter(typeof(StringLongJsonConverter))]
    public readonly struct stringlong : IEquatable<stringlong>
    {
        private readonly long _value;
        private readonly string? _stringValue;
        public long Value => _value;
        public string StringValue => _stringValue ?? _value.ToString();
        private readonly bool isNull =false;
        public stringlong(long value)
        {
            _value = value;
            _stringValue = null;
            isNull = true;
        }

        public stringlong(string value)
        {
            _value = long.Parse(value);
            _stringValue = value;
            isNull = true;
        }

        public override string ToString() => StringValue;

        public static implicit operator long(stringlong id) => id._value;
        public static implicit operator stringlong(long value) => new(value);
        public static implicit operator stringlong(string value) => new(value);
        public static explicit operator string(stringlong id) => id.StringValue;
        public bool IsNull()
        {
            return isNull;
        }
        public bool Equals(stringlong other) => _value == other._value;
        public override bool Equals(object? obj) => obj is stringlong other && Equals(other);
        public override int GetHashCode() => _value.GetHashCode();
    }
    public class StringLongJsonConverter : JsonConverter<stringlong>
    {
        public override stringlong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                return new stringlong(reader.GetString()!);
            }

            if (reader.TokenType == JsonTokenType.Number)
            {
                return new stringlong(reader.GetInt64());
            }

            throw new JsonException("Invalid JSON value for stringlong.");
        }

        public override void Write(Utf8JsonWriter writer, stringlong value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.StringValue);
        }
    }
    public readonly record struct UserId(stringlong Value)
    {
        public UserId(long value) : this(new stringlong(value)) { }
        public UserId(string value) : this(new stringlong(value)) { }
        public override string ToString() => Value.ToString();
        public static implicit operator long(UserId id) => id.Value;
    }

    public readonly record struct ServerId(stringlong Value)
    {
        public ServerId(long value) : this(new stringlong(value)) { }
        public ServerId(string value) : this(new stringlong(value)) { }
        public override string ToString() => Value.ToString();
        public static implicit operator long(ServerId id) => id.Value;
    }

    public readonly record struct ChannelId(stringlong Value)
    {
        public ChannelId(long value) : this(new stringlong(value)) { }
        public ChannelId(string value) : this(new stringlong(value)) { }
        public override string ToString() => Value.ToString();
        public static implicit operator long(ChannelId id) => id.Value;
    }



}
