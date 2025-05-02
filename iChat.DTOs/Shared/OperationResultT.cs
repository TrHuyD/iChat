using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iChat.DTOs.Shared
{
    class OperationResultT<T> :OperationResult
    {
        public T? Value { get; set; }
        public static OperationResultT<T> Ok(T value) => new()
        {
            Success = true,
            Value = value
        };
    }
}
