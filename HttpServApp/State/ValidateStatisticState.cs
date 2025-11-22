using HttpServApp.Builder;
using HttpServApp.Models;
using System.Net.Sockets;

namespace HttpServApp.State
{
  /// <summary>
  /// Стан пiсля валiдацiї: валiдний запит статистичних даних
  /// </summary>
  internal class ValidateStatisticState : IState
  {
    public void ProcessingHandler(HttpRequest httpRequest, Socket socket)
    {
      // Будуємо вiдповiдь за допомогою методiв iнтерфейсу IBuilder
      IBuilder builder = new BuilderStat(httpRequest);
      byte[] sendBytes = [
        .. builder.BuildVersion(),
        .. builder.BuildStatus(),
        .. builder.BuildHeaders(),
        .. builder.BuildContentBody()
       ];

      // Вiдсилаємо вiдповiдь клiєнту
      httpRequest.SendResponseByte(socket, sendBytes);
      Console.WriteLine($"HttpRequest state: ValidateStatisticState");

      // Перехiд у новий стан: пiсля вiдправки вiдповiдi клiєнту
      httpRequest.TransitionTo(new SendedState(), socket);

    }
  }
}
