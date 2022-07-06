using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Core.Core.Exceptions
{
    public class ApiException : System.Exception
    {
        public ApiException() : base()
        { }
        public ApiException(string message) : base(message)
        { }

        public ApiException(string message, params object[] args) : base(string.Format(CultureInfo.CurrentCulture,
            message, args))
        {

        }
    }
}
