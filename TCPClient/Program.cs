using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class TCPClient
{
    private String server;
    private Int32 port;

    public TCPClient()
    {
        server = "127.0.0.1";
        port = 13000;
    }

    public async Task StartAsync()
    {
        try
        {
            TcpClient client = new TcpClient(server, port);
            Console.WriteLine("Подключено к серверу {0}:{1}", server, port);

            NetworkStream stream = client.GetStream();

            // Отправляем сообщение серверу
            String message = "Привет от клиента";
            byte[] msg = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(msg, 0, msg.Length);
            Console.WriteLine("Отправлено сообщение: {0}", message);

            // Получаем ответ от сервера
            msg = new byte[256];
            String response = String.Empty;
            Int32 bytes = await stream.ReadAsync(msg, 0, msg.Length);
            response = Encoding.UTF8.GetString(msg, 0, bytes);
            Console.WriteLine("Получен ответ: {0}", response);

            stream.Close();
            client.Close();
            Console.WriteLine("Соединение закрыто.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка подключения: {0}", ex.Message);
        }
    }
}

public class ClientProgram
{
    public static async Task Main()
    {
        TCPClient client = new TCPClient();
        await client.StartAsync();
    }
}
