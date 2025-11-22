using HttpServApp.Models;
using HttpServApp.State;
using HttpServApp.Processing;
using System.Net;

namespace HttpServApp.Faсtory
{
  /// <summary>
  /// Клас CreatorRequestPage реалізує інтерфейс ICreatorRequest для створення 
  /// об'єкту класу HttpRequestPage (запит Web-сторiнки) та його початкового стану ValidatePageState
  /// </summary>
  internal class CreatorRequestPage : ICreatorRequest
  {
    public (HttpRequest, IState) FactoryMethod(Validator validator, Repository repository)
    {
      try
      {
        HttpRequestPage httpRequest = new HttpRequestPage(
            repository, validator.GetStringRequest(), DateTime.Now,
            validator.GetVersionRequest(), validator.GetMethodRequest(),
            validator.RemoteEndPoint, validator.LocalEndPoint, validator.GetContentTypeRequest(),
            validator.GetFileRequest());

        Console.WriteLine($"Processing: запит сторiнки {httpRequest.Path}!");

        return (httpRequest, new ValidatePageState());
      }
      catch (WebException webE)
      {
        HttpRequestInvalid httpRequest = new HttpRequestInvalid(
          repository, validator.GetStringRequest(), DateTime.Now,
          validator.RemoteEndPoint, validator.LocalEndPoint, $"{webE.Message}");
        Console.WriteLine($"CreatorRequestPage WebException: {webE.Message}");
        return (httpRequest, new InvalidState());
      }
      catch (Exception exc)
      {
        HttpRequestInvalid httpRequest = new HttpRequestInvalid(
          repository, validator.GetStringRequest(), DateTime.Now,
          validator.RemoteEndPoint, validator.LocalEndPoint, exc.Message);
        Console.WriteLine($"CreatorRequestPage Exception: {exc.Message}");
        return (httpRequest, new InvalidState());
      }
    }

  }
}
