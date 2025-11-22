using HttpServApp.Models;
using System.Text;

namespace HttpServApp.Builder
{
  /// <summary>
  /// Builder-клас побудови вiдповiдi на не валiдний запит
  /// </summary>
  internal class BuilderInvalid : IBuilder
  {
    private readonly HttpRequestInvalid httpRequestInvalid;
    public BuilderInvalid(HttpRequest httpRequest)
    {
      httpRequestInvalid = httpRequest as HttpRequestInvalid ?? throw new ArgumentNullException(nameof(httpRequestInvalid));
    }
    public byte[] BuildVersion() => Encoding.UTF8.GetBytes($"HTTP/{httpRequestInvalid.Version ?? "1.1"} ");

    public byte[] BuildStatus() => Encoding.UTF8.GetBytes($"{(int)httpRequestInvalid.Status} {httpRequestInvalid.Status} ");

    public byte[] BuildHeaders() => Encoding.UTF8.GetBytes(
        $"\nContent-Type:{httpRequestInvalid.ContentTypeRequest ?? "text/html"};charset=UTF-8;" +
        $"\nConnection: close\n");

    public byte[] BuildContentBody()
    {
      Console.WriteLine(httpRequestInvalid.Message);
      httpRequestInvalid.Message = $"<p style='font-size: 16px; font-family: Verdana; color: red'>{httpRequestInvalid.Message}</p>";
      httpRequestInvalid.Response = new HttpResponse(
          DateTime.Now, Encoding.UTF8.GetByteCount(httpRequestInvalid.Message ?? string.Empty));

      return Encoding.UTF8.GetBytes(
          $"Content-Length:{httpRequestInvalid.Response?.ContentLength ?? 0}\n\n" +
          $"{httpRequestInvalid.Message ?? string.Empty}");
    }

  }
}
