using HttpServApp.Models;
using HttpServApp.Processing;
using HttpServApp.State;
using System.Net;

namespace HttpServApp.Faсtory
{
  /// <summary>
  /// Клас CreatorRequestStat реалізує інтерфейс ICreatorRequest для створення 
  /// об'єкту класу HttpRequestStat (запит статистики за перiод) та його початкового стану ValidateStatisticState
  /// </summary>
  internal class CreatorRequestStat : ICreatorRequest
  {
    public (HttpRequest, IState) FactoryMethod(Validator validator, Repository repository)
    {
      try
      {
        HttpRequestStat httpRequest = new HttpRequestStat(
            repository, validator.GetStringRequest(), DateTime.Now,
            validator.GetVersionRequest(), validator.GetMethodRequest(),
            validator.RemoteEndPoint, validator.LocalEndPoint, validator.GetContentTypeRequest(),
            validator.GetDateBegRequest(), validator.GetDateEndRequest(),
            validator.GetKeyAuthorization());
        Console.WriteLine($"Processing: запит статистики за перiод " +
             $"{httpRequest.DateBeg}-{httpRequest.DateEnd}!");

        return (httpRequest, new ValidateStatisticState());

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
