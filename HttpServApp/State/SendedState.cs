using HttpServApp.Models;
using System.Net.Sockets;


namespace HttpServApp.State
{
  /// <summary>
  /// Стан пiсля вiдправки даних клiєнту: необхiдно зберегти iнформацiю про запит в БД
  /// </summary>
  internal class SendedState : IState
  {
    public void ProcessingHandler(HttpRequest httpRequest, Socket socket)
    {
      // Викликаємо метод запису даних про запит до БД
      httpRequest.Repository.SaveToDB(httpRequest, '+');

      // Перехiд у фiнальний стан
      httpRequest.TransitionTo(new DoneState(), socket);
    }
  }
}
