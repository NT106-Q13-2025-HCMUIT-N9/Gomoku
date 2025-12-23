using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku_Client.Model
{
    enum State
    {
        Ready,
        InMatch
    }
    internal class UserState
    {
        public static State currentState { get; set; } = State.Ready;
    }
}
