using HttpServApp.Models;
using System.Net.Sockets;

namespace HttpServApp.Processing
{
  /// <summary>
  /// Клас обробки запиту в багатопотоковому режимi
  /// </summary>
  internal class MultiThreadProcessing: RequestProcessing
  {
    public void Process(Repository repository, Socket clientSocket)
    {
      // Створення потоку обробки даних
      Thread workThread = new Thread(args => DoWork(args as ProcessingArgs));
      // Старт потоку з передачою параметрiв
      workThread.Start(
        new ProcessingArgs() 
        { 
          Repository = repository, 
          Socket = clientSocket 
        });
    }
  }
}
