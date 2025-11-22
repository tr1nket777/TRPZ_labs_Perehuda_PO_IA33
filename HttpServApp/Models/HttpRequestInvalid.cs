namespace HttpServApp.Models
{

  /// <summary>
  /// Клас, що описує помилковий запит до серверу
  /// </summary>    
  internal class HttpRequestInvalid : HttpRequest
  {

    public HttpRequestInvalid(Repository repository, string stringRequest,
    DateTime dateTimeRequest, string ipAddressClient, string ipAddressServer, string message, long idRequest = -1)
        : base(repository, stringRequest, dateTimeRequest, null, null, ipAddressClient, ipAddressServer, null, message, idRequest)
    {
      Status = StatusEnum.BAD_REQUEST;
    }

  }
}