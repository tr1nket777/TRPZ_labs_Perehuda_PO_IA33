using HttpServApp.State;
using System.Net.Sockets;

namespace HttpServApp.Models
{
  public enum StatusEnum
  {
    PROCESSING = 0,
    OK = 200,
    REDIRECT = 308,
    BAD_REQUEST = 400,
    UNAUTHORIZED = 401,
    NOT_FOUND = 404,
    NOT_ALLOWED = 405,
    BAD_SERVER = 500
  }

  public enum TypeRequestEnum
  {
    НЕ_ВИЗНАЧЕНО = 0,
    СТОРІНКА = 1,
    СТАТИСТИКА = 2
  }

  /// <summary>
  /// Базовий клас, що описує запит до серверу
  /// </summary>    
  internal class HttpRequest
  {
    // Посилання на поточний стан об'єкта
    private IState? state = null;
    // Посилання на репозиторiй, де зберiгається об'єкт
    public Repository Repository { get; }
    // Строка запиту
    public string StringRequest { get; }
    // Ідентифікатор запиту
    public long IdRequest { get; set; } = -1;
    // Дата/час запиту
    public DateTime DateTimeRequest { get; } = DateTime.Now;
    // Тип запиту: СТОРІНКА / СТАТИСТИКА
    public TypeRequestEnum TypeRequest { get; set; } = TypeRequestEnum.НЕ_ВИЗНАЧЕНО;
    // Версія протоколу HTTP
    public string? Version { get; }
    // Метод запиту
    public string? Method { get; }
    // IP-адреса клієнта
    public string IpAddressClient { get; } = string.Empty;

    // IP-адреса сервера
    public string IpAddressServer { get; } = string.Empty;

    // Http-статус обробки запиту, початково статус запиту iнiцiалiзується значенням PROCESSING:
    // вiдповiдь не сформована i не вiдправлена
    public StatusEnum Status { get; set; } = StatusEnum.PROCESSING;
    public string? StatusStr { get => Status.ToString(); }
    // Тип змісту запиту
    public string? ContentTypeRequest { get; }
    // Повідомлення про помилку, якщо статус запиту не дорівнює StatusEnum.OK
    public string? Message { get; set; }
    // Об'єкт відповіді
    public HttpResponse? Response { get; set; }

    public HttpRequest(Repository repository, string stringRequest, 
        DateTime dateTimeRequest,
        string? version, string? method, string ipAddressClient, string ipAddressServer,
        string? contentType, string? message = "", long idRequest = -1)
    {
      // Заповнили всi поля
      StringRequest = stringRequest;
      DateTimeRequest = dateTimeRequest;
      Version = version;
      Method = method;
      IpAddressClient = ipAddressClient;
      IpAddressServer = ipAddressServer;
      ContentTypeRequest = contentType;
      Message = message;
      IdRequest = idRequest;

      Repository = repository;
    }

    // Вiдправка вiдповiдi клiєнту
    public void SendResponseByte(Socket socket, byte[] data)
    {
      if (Response == null) return;
      try
      {
        if (socket != null)
        {
          socket.Send(data);
          // Встановлюємо ознаку вiдправленої вiдповiдi 
          Response.StatusSend = 1;
        }
        else
        {
          Console.WriteLine("Сформована вiдповiдь не вiдправлена: Socket=null");
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine($"Сформована вiдповiдь не вiдправлена: {exc.Message}");
      }
    }

    // Метод дозволяє змiнювати стан об'єкта пiд час виконання
    public void TransitionTo(IState state, Socket socket)
    {
      Console.WriteLine($"HttpRequest state: Transition to {state.GetType().Name}.");
      this.state = state;
      // Виклик методу обробки нового стану запиту
      this.state.ProcessingHandler(this, socket);
    }
  }
}