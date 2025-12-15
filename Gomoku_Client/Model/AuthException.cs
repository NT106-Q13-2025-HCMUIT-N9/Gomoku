using Firebase.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Client.Model
{

    public class AuthException : Exception
    {
        public AuthException(string message)
        : base(message) { }
    }
}
