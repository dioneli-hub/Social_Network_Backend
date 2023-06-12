using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Domain
{
    public class ServiceResponse<T>
    {
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; } = true;
    }
}
