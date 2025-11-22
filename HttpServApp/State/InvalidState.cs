using HttpServApp.Builder;
using HttpServApp.Models;
using System.Net.Sockets;

namespace HttpServApp.State
{
  /// <summary>
  /// Стан пiсля валiдацiї: невалiдний запит 
  /// </summary>
  internal class InvalidState : IState
  {
    public void ProcessingHandler(HttpRequest httpRequest, Socket socket)
    {
      // Будуємо вiдповiдь за допомогою методiв iнтерфейсу IBuilder
      IBuilder builder = new BuilderInvalid(httpRequest);
      byte[] sendBytes = [
        .. builder.BuildVersion(),
        .. builder.BuildStatus(),
        .. builder.BuildHeaders(),
        .. builder.BuildContentBody()
       ];

      // Вiдсилаємо вiдповiдь клiєнту
      httpRequest.SendResponseByte(socket, sendBytes);
      Console.WriteLine($"HttpRequest state: InvalidState");

      // Перехiд у новий стан: пiсля вiдправки вiдповiдi клiєнту
      httpRequest.TransitionTo(new SendedState(), socket);
    }
  }
}
