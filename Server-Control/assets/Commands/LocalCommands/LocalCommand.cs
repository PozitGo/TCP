using System;
using System.Data;
using System.Threading.Tasks;

namespace Server_Control.assets.Commands.LocalCommands
{
    interface ILocalCommand
    {
        Task ExecuteAsync();
    }

    public class GetClientList : ILocalCommand
    {
        public Task ExecuteAsync()
        {
            if (Server.clientList.Count != 0)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nСписок подключенных клиентов:");
                Console.ResetColor();

                Console.ForegroundColor = ConsoleColor.DarkMagenta;

                foreach (var client in Server.clientList)
                {
                    Console.WriteLine($"Клиент IP {client.Value.Client.RemoteEndPoint}, UUID {client.Key} подключен.");
                }

                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("\nНе 1 клиент ещё не подключён.");
                Console.ResetColor();
            }

            return Task.CompletedTask;
        }
    }
}
