using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server_Control.assets
{
    public static class MessageServer
    {
        public static async Task SendMessageToClient(TcpClient client, string message)
        {
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(message);
                string base64Message = Convert.ToBase64String(buffer);
                buffer = Encoding.UTF8.GetBytes(base64Message);

                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(buffer, 0, buffer.Length);
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }
        }

        public static async Task<string> ReadClientMessage(TcpClient client)
        {
            NetworkStream stream = client.GetStream();

            try
            {
                while (true)
                {
                    if (stream.DataAvailable)
                    {
                        byte[] buffer = new byte[4096];
                        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        string base64Message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        byte[] messageBytes = Convert.FromBase64String(base64Message);
                        return Encoding.UTF8.GetString(messageBytes, 0, messageBytes.Length);
                    }
                }
            }
            catch (Exception)
            {
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                Console.WriteLine($"\nКлиент {client.Client.RemoteEndPoint} отключился c UUID {Server.clientList.FirstOrDefault(x => x.Value == client).Key} в {DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")}.");
                Console.ResetColor();
                Server.clientList.TryRemove(Server.clientList.FirstOrDefault(x => x.Value == client).Key, out _);
            }

            return null;
        }

    }
}
