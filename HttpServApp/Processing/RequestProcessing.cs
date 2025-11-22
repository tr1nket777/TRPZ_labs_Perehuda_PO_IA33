using HttpServApp.Faсtory;
using HttpServApp.Mediator;
using HttpServApp.Models;
using HttpServApp.State;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace HttpServApp.Processing
{
  internal class ProcessingArgs
  {
    public required Repository Repository { get; set; }
    public required Socket Socket { get; set; }
  }
  internal class RequestProcessing
  {
    public IMediator? Mediator { get; set; }

    /// <summary>
    /// Метод, що виконує обробку даних запиту 
    /// (вiн є реентерабельним, тобто потокобезпечним: не залежить вiд стану об'єкта)
    /// </summary>
    protected virtual void DoWork(ProcessingArgs threadArgs)
    {
      Socket socket = threadArgs.Socket;
      // Зчитуємо дані сокета в строку
      string strRequest = GetStringRequest(socket);

      Validator validator = new Validator(strRequest, socket.LocalEndPoint, socket.RemoteEndPoint);
      // Об'єкт фабрики, метод якого створює запит необхiдного типу
      ICreatorRequest? creator;
      try
      {
        // Аналiзуємо строку запиту
        if (string.IsNullOrEmpty(validator.GetStringRequest()))
        {
          //Console.WriteLine("EMPTY STRING!!!!!!!!!!!!!!!!!!!!");
          return;
        }

        // Визначаємо тип запиту
        string typeRequest = validator.GetTypeRequest();

        switch (typeRequest)
        {
          // Запит сторiнки
          case "page":
            {
              creator = new CreatorRequestPage();
              break;
            }
          // Запит статистики
          case "stat":
            {
              creator = new CreatorRequestStat();
              break;
            }
          default:
            throw new WebException("Невизначений тип запиту", WebExceptionStatus.ProtocolError);

        }
      }
      catch (WebException)
      {
        creator = new CreatorRequestInvalid();
      }

      if (creator != null)
      {
        // За допомогою фабрики створюємо tuple, що мiстить об'єкт запиту та його початковий стан.
        (HttpRequest httpRequest, IState startState) = creator.FactoryMethod(validator, threadArgs.Repository);
        // Запускаємо ланцюжок переходу станiв об'єкту httpRequest
        httpRequest.TransitionTo(startState, socket);
      }

    }

    /// <summary>
    /// Отримання строки запиту з потоку байтiв у сокетi
    /// </summary>
    /// <param name="socket"></param>
    /// <returns></returns>
    private static string GetStringRequest(Socket socket)
    {
      // Встановлюємо розмiр блоку даних
      byte[] bufferBytes = new byte[1024];
      // Зчитуємо данi
      try
      {
        socket.ReceiveTimeout = 60 * 1000;
        string strReceiveRequest = "";
        // Цикл, поки не досягли закiнчення масиву
        do
        {
          int bytes = socket.Receive(bufferBytes, bufferBytes.Length, SocketFlags.None);
          strReceiveRequest += Encoding.UTF8.GetString(bufferBytes, 0, bytes);
        }
        while (socket.Available > 0);

        Console.WriteLine($"==== Змiст запиту ({socket.RemoteEndPoint}):\n{strReceiveRequest}");
        return strReceiveRequest;
      }
      catch (Exception ex)
      {
        //Console.WriteLine($"==== GetStringRequest exception ({socket.RemoteEndPoint}):\n{ex.Message}");
        return string.Empty;
      }
    }
  }
}
