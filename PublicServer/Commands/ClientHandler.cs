using PublicServer.Decryptor;
using PublicServer.JSON;
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace PublicServer.Commans
{
    public class ClientHandler
    {
        private readonly TcpClient client;
        private readonly JsonEncryptionService encryptionService;

        public ClientHandler(TcpClient client, JsonEncryptionService encryptionService)
        {
            this.encryptionService = encryptionService;
            this.client = client;
        }

        public async Task Handle()
        {
            try
            {
                (RSAParameters publicKey, RSAParameters privateKey) = RsaKeyGenerator.GenerateKeyPair();
                RsaDecryptor decryptor = new RsaDecryptor(privateKey);

                await Server.SendMessageToClient(client, RSAKeySerializer.SerializeToString(publicKey));

                while (decryptor.Decrypt(await Server.ReadClientMessage(client)) != encryptionService.DecryptJsonFromFile())
                {
                    if (client.Connected)
                    {
                        await Server.SendMessageToClient(client, "$error");
                        (publicKey, privateKey) = RsaKeyGenerator.GenerateKeyPair();
                        decryptor = new RsaDecryptor(privateKey);
                        await Server.SendMessageToClient(client, RSAKeySerializer.SerializeToString(publicKey));
                    }
                    else
                    {
                        Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
                        client.Close();
                    }

                }

                await Server.SendMessageToClient(client, "$success");

                NetworkStream stream = client.GetStream();

                while (client.Connected)
                {
                    ICommand commandHandler = default;
                    Task.Factory.StartNew(async() => commandHandler = await GetCommandHandler(client)).Wait();
                    
                    if(commandHandler != null)
                    {
                        await commandHandler.ExecuteAsync(stream);
                    }
                }

                Console.WriteLine($"Клиент - {client.Client.RemoteEndPoint} отключился");
                client.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обработке соединения: {ex.Message}");
            }
        }

        private async Task<ICommand> GetCommandHandler(TcpClient client)
        {
            if (client.Connected)
            {
                string command = await Server.ReadClientMessage(client);

                switch (command)
                {
                    case "$download":

                        string SendPath = await Server.ReadClientMessage(client);

                        if(SendPath is "$exit")
                        {
                            return null;
                        }
                        else if (File.Exists(SendPath) || Directory.Exists(SendPath))
                        {
                            return new SendCommand(SendPath);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для отправки клиенту некорректен - {SendPath}");
                        }

                        break;

                    case "$upload":

                        string SavePath = await Server.ReadClientMessage(client);

                        if(SavePath is "$exit")
                        {
                            return null;
                        }
                        else if (Directory.Exists(SavePath))
                        {
                            return new ReciveCommand(SavePath);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для получения файла от клиента некорректен - {SavePath}");
                        }

                        break;

                    case "$delete":

                        string ReciveDeletePath = await Server.ReadClientMessage(client);

                        if(ReciveDeletePath is "$exit")
                        {
                            return null;
                        }
                        else if (Directory.Exists(ReciveDeletePath) || File.Exists(ReciveDeletePath))
                        {
                            return new DeleteReciveCommand(ReciveDeletePath);
                        }
                        else
                        {
                            Console.WriteLine($"Путь для удаления файла от клиента некорректен - {ReciveDeletePath}");
                        }

                        break;
                    default:
                        Console.WriteLine($"Неизвестная команда: {command}");
                        return null;
                }
            }

            return null;
        }

    }

}
