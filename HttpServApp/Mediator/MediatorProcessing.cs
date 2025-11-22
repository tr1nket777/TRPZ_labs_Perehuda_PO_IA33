
using HttpServApp.Models;
using HttpServApp.Processing;
using System.Net.Sockets;

namespace HttpServApp.Mediator
{
  /// <summary>
  /// Посередник обробки запиту вiд Http-клiєнта
  /// </summary>
  internal class MediatorProcessing: IMediator
  {
    private readonly Listener listener;
    private readonly Repository repository;
    private readonly MultiThreadProcessing multiThreadProcessing;
    private readonly SingleThreadProcessing singleThreadProcessing;

    /// <summary>
    /// Конструктор об'єкта, у параметрах заданi всi об'єкти-колеги, що взаємодiють з медiатором
    /// </summary>
    /// <param name="listener"></param>
    /// <param name="repository"></param>
    /// <param name="multiThreadProcessing"></param>
    /// <param name="singleThreadProcessing"></param>
    public MediatorProcessing(Listener listener, Repository repository, 
      MultiThreadProcessing multiThreadProcessing,
      SingleThreadProcessing singleThreadProcessing)
    {
      this.listener = listener;
      this.listener.Mediator = this;

      this.repository = repository;
      this.repository.Mediator = this;

      this.multiThreadProcessing = multiThreadProcessing;
      this.multiThreadProcessing.Mediator = this;
      
      this.singleThreadProcessing = singleThreadProcessing; 
      this.singleThreadProcessing.Mediator = this;
    }

    /// <summary>
    /// Метод, що використовується компонентами для сповiщення посередника про рiзнi подiї.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="target"></param>
    public void Notify(object sender, object target)
    {
      // Якщо повiдомлення надiйшло вiд Listener
      if (sender is Listener)
      {
        // як цiльовий об'єкт переданий Socket
        if (target is Socket clientSocket)
        {
          // Зчитуємо конфiгурацiю застосунку.
          // Якщо параметр багатопотоковостi встановлений, то для обробки запиту використовуємо об'єкт типу MultiThreadProcessing,
          // що запускає окремий потiк обробки запиту
          if (Configuration.MultiThread)
            multiThreadProcessing.Process(repository, clientSocket);
          // Якщо параметр багатопотоковостi НЕ встановлений, то для обробки запиту використовуємо об'єкт типу SingleThreadProcessing,
          // що виконує обробку запиту в основному потоцi
          else
            singleThreadProcessing.Process(repository, clientSocket);
        }
        else
        {
          Console.WriteLine(target.ToString());
        }

      }
      // Якщо сповiщення прийшло вiд Repository, просто виводимо цiльове повiдомлення
      else if (sender is Repository) 
      {
          Console.WriteLine(target.ToString());
      }
    }
  }
}
