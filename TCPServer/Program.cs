using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TCPServer
{
    private TcpListener server;
    private Int32 port;
    private IPAddress localAddr;

    public TCPServer()
    {
        localAddr = IPAddress.Loopback;
        port = 13000;
        server = new TcpListener(localAddr, port);
    }

    public async Task StartAsync()
    {
        server.Start();
        Console.WriteLine("Сервер запущен на {0}:{1}", localAddr, port);

        while (true)
        {
            Console.WriteLine("Ожидание подключения клиента...");
            TcpClient client = await server.AcceptTcpClientAsync();
            Console.WriteLine("Клиент подключен!");

            await Task.Factory.StartNew(async ()  => {
               await HandleClientRequestAsync(client);
            });
        }
    }

    private async Task HandleClientRequestAsync(object obj)
    {
        TcpClient client = (TcpClient)obj;

        Byte[] bytes = new Byte[256];
        String data = null;
        NetworkStream stream = client.GetStream();

        int i;
        while ((i = await stream.ReadAsync(bytes, 0, bytes.Length)) != 0)
        {
            data = Encoding.UTF8.GetString(bytes, 0, i);
            Console.WriteLine("Получено сообщение: {0}", data);

            // Отправляем сообщение обратно клиенту
            byte[] buffer = Encoding.UTF8.GetBytes("Привет, от сервера");
            await stream.WriteAsync(buffer, 0, buffer.Length);
            Console.WriteLine("Отправлено сообщение: {0}", data);
        }

        client.Close();
        Console.WriteLine("Соединение закрыто.\n");
    }
}

public class ServerProgram
{
    public static async Task Main()
    {
        TCPServer server = new TCPServer();
        await server.StartAsync();
    }
}
