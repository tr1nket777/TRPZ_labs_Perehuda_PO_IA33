using HttpServApp.Models;
using System.Net.Sockets;

namespace HttpServApp.p2p
{
  /// <summary>
  /// Клас, що реалізує перенаправлення запиту на інший резервний сервер
  /// </summary>
  internal class P2pRedirect
  {
    private string host = Configuration.RemoteHost ?? "127.0.0.1";
    private int port = Configuration.RemotePort ?? 80;
    public P2pRedirect() { }

    public P2pRedirect(string host, int port) 
    {
      this.host = host;
      this.port = port;
    }

    /// <summary>
    /// Метод перенаправлення даних запиту на резервний сервер 
    /// та відправки відповіді від нього клієнту
    /// </summary>
    /// <param name="sendMsg"></param>
    /// <param name="requestSocket"></param>
    /// <returns></returns>
    public int HandleRedirect(byte[] sendMsg, Socket requestSocket)
    {
      // Створюємо сокет для перенаправлення даних запиту на резервний сервер
      Socket redirectSocket = new Socket(SocketType.Stream, ProtocolType.Tcp);
      int sendCount = 0;
      
      try
      {
        // Встановлюємо з'єднання
        redirectSocket.Connect(host, port);
        if (redirectSocket.Connected)
        {
          // Відправляємо на резервний сервер масив байтів запиту 
          int sentCount = redirectSocket.Send(sendMsg);
          redirectSocket.ReceiveTimeout = 60 * 1000;

          // Повертаємо змiст відповіді від віддаленого серверу
          // Встановлюємо розмiр блоку даних
          byte[] bufferBytes = new byte[1024 * 16];
          // Цикл, поки не досягли закiнчення масиву
          do
          {
            // Читаємо порцію даних
            int readByteCount = redirectSocket.Receive(bufferBytes, bufferBytes.Length, SocketFlags.None);
            // Одразу віддаємо цю порцію клієнту (в requestSocket)
            int sendByteCount = requestSocket.Send(bufferBytes, 0, readByteCount, SocketFlags.None);
            // Підбиваємо підсумки переданих байтів
            sendCount += sendByteCount;
            if (readByteCount != sendByteCount)
            {
              throw new Exception("HandleRedirect: кількість зчитаних байтів не співпадає з кількістю переданих!");
            }
            // Трошки почекаємо: може, "залишок" не встиг прийти
            if (redirectSocket.Available <= 0)
                Thread.Sleep(500);
          }
          while (redirectSocket.Available > 0);
        }
        else
        {
          throw new Exception("HandleRedirect: клієнтський сокет втратв з'єднання");
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"==== HandleRedirect exception ({host}:{port}):\n{ex.Message}");
      }
      finally
      {
        redirectSocket.Shutdown(SocketShutdown.Both);
        redirectSocket.Close();
        redirectSocket.Dispose();
      }
      return sendCount;
    }

  }
}
