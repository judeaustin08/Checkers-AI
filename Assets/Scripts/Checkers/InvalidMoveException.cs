using System;

namespace Checkers
{
    public class InvalidMoveException : Exception
    {
        public InvalidMoveException(string message = "") : base("The provided move is invalid! " + message) { }
    }
}