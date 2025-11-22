using HttpServApp.Models;
using HttpServApp.Processing;
using HttpServApp.State;

namespace HttpServApp.Faсtory
{
  /// <summary>
  /// Клас CreatorRequestInvalid реалізує інтерфейс ICreatorRequest для створення 
  /// об'єкту класу HttpRequestInvalid та його початкового стану InvalidState
  /// </summary>
  internal class CreatorRequestInvalid : ICreatorRequest
  {
    public (HttpRequest, IState) FactoryMethod(Validator validator, Repository repository)
    {
      HttpRequestInvalid httpRequest = new HttpRequestInvalid(
        repository, validator.GetStringRequest(), DateTime.Now,
        validator.RemoteEndPoint, validator.LocalEndPoint, "Невизначений тип запиту.");
      Console.WriteLine("Processing: Невизначений тип запиту!");

      return (httpRequest, new InvalidState());
    }
  }
}
