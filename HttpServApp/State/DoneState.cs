using HttpServApp.Models;
using System.Net.Sockets;

namespace HttpServApp.State
{
  /// <summary>
  /// Фiнальний стан об'єкта
  /// </summary>
  internal class DoneState : IState
  {
    public void ProcessingHandler(HttpRequest httpRequest, Socket socket)
    {
      // Цей стан - останнiй, перехiд не потрiбен
      return;
    }
  }
}
