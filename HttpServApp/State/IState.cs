using HttpServApp.Models;
using System.Net.Sockets;

namespace HttpServApp.State
{
  // iнтерфейс, що вiдповiдає за стан об'єкта HttpRequest
  interface IState
  {
    public void ProcessingHandler(HttpRequest httpRequest, Socket socket);
  }
}
