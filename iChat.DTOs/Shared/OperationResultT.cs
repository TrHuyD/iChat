using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Shared
{
    public class OperationResultT<T> :OperationResult
    {
        public T? Value { get; set; }
        public static OperationResultT<T> Ok(T value) => new()
        {
            Success = true,
            Value = value
        };
        public static OperationResultT<T> Fail(string code, string message) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };
        public static OperationResultT<T> FailFrom<T, TSource>(OperationResultT<TSource> source)
        {
            return new OperationResultT<T>
            {
                Success = false,
                ErrorCode = source.ErrorCode,
                ErrorMessage = source.ErrorMessage
            };
        }
        public OperationResultT<TNew> FailAs<TNew>()
        {
            return new OperationResultT<TNew>
            {
                Success = this.Success,
                ErrorCode = this.ErrorCode,
                ErrorMessage = this.ErrorMessage
            };
        }

    }
    public class OperationResultBool : OperationResultT<bool>
    {
        public static OperationResultBool Ok(bool value) => new() { Success = true, Value = value };
        public  static OperationResultBool Fail(string code, string message) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };

    }
    public class OperationResultString : OperationResultT<string>
    {
        public static OperationResultString Ok(string value) => new() { Success = true, Value = value };
        public static OperationResultString Fail(string code, string message) => new()
        {
            Success = false,
            ErrorCode = code,
            ErrorMessage = message
        };

    }
}
