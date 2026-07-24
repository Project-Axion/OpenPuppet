using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenPuppet.SDK.Exceptions
{
    public class NotImplementedInterfaceException : Exception
    {
        public override string Message { get; }
        public NotImplementedInterfaceException(string message)
        {
            Message = message;
        }
    }
    
    public class NotRegisteredException : Exception
    {
        public override string Message { get; }
        public NotRegisteredException(string message)
        {
            Message = message;
        }
    }
}
