using System.Web;

namespace HttpServApp.Models
{
  /// <summary>
  /// Цей клас мiстить iнформацiю про запит Web-сторiнки та вiдповiдь 
  /// </summary>
  internal class HttpRequestPage : HttpRequest
  {
    public string Path { get; set; } = string.Empty;
    public int ContentLength { get; }

    public HttpRequestPage(Repository repository, string stringRequest,
        DateTime dateTimeRequest,
        string version, string method,
        string ipAddressClient, string ipAddressServer, string contentType,
        string path, string? message = null,long idRequest = -1)

        : base(repository, stringRequest, dateTimeRequest, version, method, 
            ipAddressClient, ipAddressServer, contentType, message, idRequest)
    {
      Path = HttpUtility.UrlDecode(path);
      TypeRequest = TypeRequestEnum.СТОРІНКА;
    }
  }
}
