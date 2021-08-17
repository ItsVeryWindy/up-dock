using System;

namespace UpDock.Git.Drivers
{
    public class PushException : Exception
    {
        public string Reference { get; }

        public PushException(string reference, string message) : base(message)
        {
            Reference = reference;
        }
    }
}
