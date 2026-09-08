using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiWarehouseInventory.Domain.Exceptions
{
    public abstract class BaseException : Exception
    {
        public string ErrorCode { get; }
        protected BaseException(string message) : base(message) { }
        protected BaseException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public class DomainException : BaseException
    {
        public DomainException(string message) : base("DOMAIN_ERROR", message) { }
        public DomainException(string message, Exception innerException) : base("DOMAIN_ERROR", message) { }
    }
}
