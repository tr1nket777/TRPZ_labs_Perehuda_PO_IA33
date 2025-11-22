using HttpServApp.Builder;
using HttpServApp.Models;
using HttpServApp.p2p;
using System.Net.Sockets;
using System.Text;

namespace HttpServApp.State
{
  /// <summary>
  /// Стан пiсля валiдацiї: валiдний запит даних Web-сторiнки
  /// </summary>
  internal class ValidatePageState : IState
  {
    public void ProcessingHandler(HttpRequest httpRequest, Socket socket)
    {
      // Будуємо вiдповiдь за допомогою методiв iнтерфейсу IBuilder
      IBuilder builder = new BuilderPage(httpRequest);
      byte[] sendBytes = [
        .. builder.BuildVersion(),
        .. builder.BuildStatus(),
        .. builder.BuildHeaders(),
        .. builder.BuildContentBody()
       ];

      // Якщо статус обробки запиту сторінки StatusEnum.REDIRECT
      // і заданий резервний віддалений сервер, передаємо запит цьому серверу (модель взаємодії peer-to-peer)
      if (httpRequest.Status == StatusEnum.REDIRECT)
      {
        P2pRedirect p2PRedirect = new P2pRedirect(Configuration.RemoteHost, Configuration.RemotePort ?? 80);
        p2PRedirect.HandleRedirect(Encoding.UTF8.GetBytes(httpRequest.StringRequest), socket);
      }
      else
      { 
        // Вiдсилаємо вiдповiдь клiєнту
        httpRequest.SendResponseByte(socket, sendBytes);
      }
      Console.WriteLine($"HttpRequest state: ValidatePageState");

      // Перехiд у новий стан: пiсля вiдправки вiдповiдi клiєнту
      httpRequest.TransitionTo(new SendedState(), socket);
    }
  }
}
