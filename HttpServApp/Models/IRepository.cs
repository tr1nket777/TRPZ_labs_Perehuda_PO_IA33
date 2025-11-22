namespace HttpServApp.Models
{
  interface IRepository
  {
    string ConnStr { get; }
    List<HttpRequest> Requests { get; }
    public void AddRequest(HttpRequest request);

    public void RemoveRequest(HttpRequest request);

    public void UpdateRequest(HttpRequest request);

    public HttpRequest? GetRequestById(long idRequest);

    public List<HttpRequest> GetRequestsByPeriod(DateTime dateBeg, DateTime dateEnd);

    public void SaveToDB(HttpRequest httpRequest, char typeOper);
  }
}
