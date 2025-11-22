using HttpServApp.Mediator;
using HttpServApp.Models;
using HttpServApp.Processing;
using System.ServiceProcess;

namespace HttpServApp
{
  partial class ServiceHttp : ServiceBase
  {
    // Ознака наявностi аргументiв: якщо true, то в режимi консольного застосунку (debug configuration)
    bool isConsoleMode = false;

    public ServiceHttp()
    {
      InitializeComponent();
    }

    protected override void OnStart(string[] args)
    {
      // TODO: Код для запуска служби
      isConsoleMode = args.Contains("run=1");

      Listener listener = new Listener();
      // Створюємо об'єкт посередника, який буде координувати дiї з обробки даних вiд Http-клiєнта
      // Змiнюючи посередника, можна змiнити принцип обробки даних (у багатопотоковому режимi чи в однопотоковому (подiєвому) режимах)
      _ = new MediatorProcessing(
            listener, new Repository(),
            new MultiThreadProcessing(), new SingleThreadProcessing());
      listener.Start();
    }

    protected override void OnStop()
    {
      // TODO: Код, що виконує пiдготовку до зупинки служби.
      if (isConsoleMode)
        Environment.Exit(1);
      else
        base.OnStop();
    }

    // запуск через консоль (для вiдладки debug)
    public void StartAsProgram(string[] args)
    {
      OnStart(args);
    }


    // зупинка через консоль (для вiдладки)
    public void StopAsProgram()
    {
      OnStop();
    }
  }
}
