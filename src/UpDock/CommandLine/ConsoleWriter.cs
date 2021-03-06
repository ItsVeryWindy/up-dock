﻿using System;

namespace UpDock.CommandLine
{
    public class ConsoleWriter : IConsoleWriter
    {
        public IConsoleWriter WriteLine(string? str)
        {
            Console.WriteLine(str);
            return this;
        }

        public IConsoleWriter WriteLine()
        {
            Console.WriteLine();
            return this;
        }
    }
}
