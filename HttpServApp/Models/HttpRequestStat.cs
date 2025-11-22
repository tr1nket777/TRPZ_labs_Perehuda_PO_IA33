namespace HttpServApp.Models
{
  /// <summary>
  /// Цей клас мiстить iнформацiю про запит статистичних даних та вiдповiдь 
  /// </summary>
  internal class HttpRequestStat : HttpRequest
  {
    public DateTime DateBeg { get; }
    public DateTime DateEnd { get; }

    public int CntRows { get; set; } = 0;
    public string? KeyAuthorization { get; }

    public HttpRequestStat(Repository repository, string stringRequest,
        DateTime dateTimeRequest,
        string version, string method,
        string ipAddressClient, string ipAddressServer, string contentType,
        DateTime dateBeg, DateTime dateEnd, string? keyAuthorization, string? message = null, long idRequest = -1)

        : base(repository, stringRequest, dateTimeRequest, version, method, 
            ipAddressClient, ipAddressServer, contentType, message, idRequest)
    {
      DateBeg = dateBeg;
      DateEnd = dateEnd;
      KeyAuthorization = keyAuthorization;
      TypeRequest = TypeRequestEnum.СТАТИСТИКА;
    }
  }
}
