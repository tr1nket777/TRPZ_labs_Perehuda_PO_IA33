namespace HttpServApp.Models
{
  internal class HttpResponse
  {
    public DateTime DateTimeResponse { get; }
    // Початкове значення = 0: вiдповiдь не вiдправлена
    public byte StatusSend { get; set; } = 0;
    // Довжина строки-вiдповiдi
    public int ContentLength { get; } = 0;
    public HttpResponse() { }

    public HttpResponse(DateTime dateTimeResponse, int contentLength)
    {
      DateTimeResponse = dateTimeResponse;
      ContentLength = contentLength;
    }

  }
}
