using HttpServApp.Models;
using HttpServApp.Processing;
using HttpServApp.State;

namespace HttpServApp.Faсtory
{
  /// <summary>
  /// Інтерфейс ICreatorRequest визначає методи 
  /// створення об'єкту запиту та його початкового стану
  /// </summary>
  internal interface ICreatorRequest
  {
    /// <summary>
    /// Створення об'єкту-кортежу (tuple) запиту та його початкового стану
    /// </summary>
    /// <returns></returns>
    public (HttpRequest, IState) FactoryMethod(Validator validator, Repository repository);

  }
}
