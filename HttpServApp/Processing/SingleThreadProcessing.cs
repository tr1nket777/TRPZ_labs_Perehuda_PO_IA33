using HttpServApp.Models;
using System.Net.Sockets;

namespace HttpServApp.Processing
{
  /// <summary>
  /// Клас обробки запиту в однопотоковому режимi (в основному потоцi)
  /// </summary>
  internal class SingleThreadProcessing: RequestProcessing
  {
    public void Process(Repository repository, Socket clientSocket)
    {
      // Запуск методу обробки запиту
      DoWork(new ProcessingArgs()
      {
        Repository = repository,
        Socket = clientSocket
      });
    }
  }
}
