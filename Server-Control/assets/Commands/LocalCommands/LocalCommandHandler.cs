using System;
using System.Collections.Generic;
using System.Text;

namespace Server_Control.assets.Commands.LocalCommands
{
    public class LocalCommandHandler
    {
        private string Command;
        private string Arguments;

        public LocalCommandHandler(string Command, string Arguments)
        {
            this.Command = Command; 
            this.Arguments = Arguments;
        }

        public LocalCommandHandler(string Command)
        {
            this.Command = Command;
        }

        public void LocalHandle()
        {
            ILocalCommand localCommand = GetLocalCommandHandler(Command, Arguments ?? default);
            localCommand?.ExecuteAsync();
        }

        private ILocalCommand GetLocalCommandHandler(string command, string arguments = null)
        {
            switch (command)
            {
                case "$clients":

                    return new GetClientList();
                

                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Локальная комманда - {command} некорректна");
                    Console.ResetColor();
                    break;
            }

            return null;
        }
    }
}
