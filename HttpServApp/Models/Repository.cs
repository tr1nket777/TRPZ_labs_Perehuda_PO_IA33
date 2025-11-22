using HttpServApp.Mediator;
using Npgsql;

namespace HttpServApp.Models
{
  internal class Repository : IRepository
  {
    public string ConnStr { get; } = Configuration.DBConnStr ?? string.Empty;
    public IMediator? Mediator { get; set; }
    public List<HttpRequest> Requests { get; } = new List<HttpRequest>();

    public Repository() { }

    public Repository(string connStr)
    {
      ConnStr = connStr;
    }

    public void AddRequest(HttpRequest request)
    {
      Requests.Add(request);
      Mediator?.Notify(this, $"Запит з Id={request.IdRequest} доданий до колекцiї. Загальна кiлькiсть {Requests.Count} запитiв.");
    }

    public void RemoveRequest(HttpRequest request)
    {
      if (Requests.Contains(request))
      {
        Requests.Remove(request);
        Mediator?.Notify(this, $"Запит з Id={request.IdRequest} видалений з колекцiї. Загальна кiлькiсть {Requests.Count} запитiв.");
      }
      else
        Mediator?.Notify(this, $"Об'єкт {request} не знайдений в колекцiї запитiв.");
    }

    public void UpdateRequest(HttpRequest request)
    {
      HttpRequest? existRequest = GetRequestById(request.IdRequest);
      if (existRequest == null)
        Mediator?.Notify(this, $"Запит для оновлення з Id={request.IdRequest} не знайдений в колекцiї запитiв.");
      else
      {
        existRequest.Status = request.Status;
        Mediator?.Notify(this, $"Статус запиту з Id={request.IdRequest} оновлено: {existRequest.Status}.");
      }
    }

    public HttpRequest? GetRequestById(long idRequest)
    {
      return Requests.Find(request => request.IdRequest == idRequest);
    }

    // Завантаження iз БД, формування колекцiї Requests
    public List<HttpRequest> GetRequestsByPeriod(DateTime dateBeg, DateTime dateEnd)
    {
      Mediator?.Notify(this, $"Отримання перелiку запитiв за перiод з {dateBeg} по {dateEnd}.");
      NpgsqlConnection connection = new NpgsqlConnection(ConnStr);
      try
      {
        Requests.Clear();
        connection.ConnectionString = ConnStr;
        connection.Open();

        NpgsqlCommand cmd = connection.CreateCommand();
        cmd.CommandType = System.Data.CommandType.Text;
        cmd.CommandText =
            @"SELECT req.""Id_Request"", req.""DateTime_Request"", req.""Type_Request"", req.""Version""," +
                @"req.""Method"", req.""Ip_Address_Client"", req.""Ip_Address_Server"", req.""Status"", req.""Content_Type_Request"", req.""Message""," +
                @"resp.""DateTime_Response"", resp.""Status_Send"", resp.""Content_Length""," +
                @"page.""Path""," +
                @"stat.""Date_Beg"", stat.""Date_End"", stat.""Cnt_Rows"", stat.""Key_Authorization""" +
             @" FROM public.""Http_Request"" req" +
             @" LEFT OUTER JOIN public.""Http_Response"" resp ON req.""Id_Request""=resp.""Id_Request""" +
             @" LEFT OUTER JOIN public.""Http_Request_Page"" page ON req.""Id_Request""=page.""Id_Request""" +
             @" LEFT OUTER JOIN public.""Http_Request_Stat"" stat ON req.""Id_Request""=stat.""Id_Request""" +
             @" WHERE req.""DateTime_Request"" BETWEEN $1 AND $2" +
             @" ORDER BY req.""DateTime_Request""";
        cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.DateTime, Value = dateBeg });
        cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.DateTime, Value = dateEnd });

        NpgsqlDataReader dataReader = cmd.ExecuteReader();
        if (dataReader.HasRows)
        {
          while (dataReader.Read())
          {
            HttpRequest request = null;
            switch ((TypeRequestEnum)Convert.ToInt16(dataReader["Type_Request"]))
            {
              case TypeRequestEnum.СТОРІНКА:
                request = new HttpRequestPage(this, string.Empty,
                    Convert.ToDateTime(dataReader["DateTime_Request"]),
                    dataReader["Version"] != DBNull.Value ? Convert.ToString(dataReader["Version"]) : null,
                    dataReader["Method"] != DBNull.Value ? Convert.ToString(dataReader["Method"]) : null,
                    Convert.ToString(dataReader["Ip_Address_Client"]),
                    Convert.ToString(dataReader["Ip_Address_Server"]),
                    dataReader["Content_Type_Request"] != DBNull.Value ? Convert.ToString(dataReader["Content_Type_Request"]) : null,
                    dataReader["Path"] != DBNull.Value ? Convert.ToString(dataReader["Path"]) : null,
                    dataReader["Message"] != DBNull.Value ? Convert.ToString(dataReader["Message"]) : null,
                    Convert.ToInt64(dataReader["Id_Request"]));
                break;

              case TypeRequestEnum.СТАТИСТИКА:
                request = new HttpRequestStat(this, string.Empty,
                    Convert.ToDateTime(dataReader["DateTime_Request"]),
                    dataReader["Version"] != DBNull.Value ? Convert.ToString(dataReader["Version"]) : null,
                    dataReader["Method"] != DBNull.Value ? Convert.ToString(dataReader["Method"]) : null,
                    Convert.ToString(dataReader["Ip_Address_Client"]),
                    Convert.ToString(dataReader["Ip_Address_Server"]),
                    dataReader["Content_Type_Request"] != DBNull.Value ? Convert.ToString(dataReader["Content_Type_Request"]) : null,
                    Convert.ToDateTime(dataReader["Date_Beg"]), Convert.ToDateTime(dataReader["Date_End"]), "",
                    dataReader["Message"] != DBNull.Value ? Convert.ToString(dataReader["Message"]) : null,
                    Convert.ToInt64(dataReader["Id_Request"]))
                {
                  CntRows = Convert.ToInt32(dataReader["Cnt_Rows"])
                };
                break;

              case TypeRequestEnum.НЕ_ВИЗНАЧЕНО:
                request = new HttpRequest(this, string.Empty,
                    Convert.ToDateTime(dataReader["DateTime_Request"]),
                    dataReader["Version"] != DBNull.Value ? Convert.ToString(dataReader["Version"]) : null,
                    dataReader["Method"] != DBNull.Value ? Convert.ToString(dataReader["Method"]) : null,
                    Convert.ToString(dataReader["Ip_Address_Client"]),
                    Convert.ToString(dataReader["Ip_Address_Server"]),
                    dataReader["Content_Type_Request"] != DBNull.Value ? Convert.ToString(dataReader["Content_Type_Request"]) : null,
                    dataReader["Message"] != DBNull.Value ? Convert.ToString(dataReader["Message"]) : null,
                    Convert.ToInt64(dataReader["Id_Request"]));
                break;
            }

            if (request != null)
            {
              // Додаємо результати вiдповiдi
              if (dataReader["DateTime_Response"] != null)
              {
                request.Status = (StatusEnum)Convert.ToInt32(dataReader["Status"]);
                request.Response = new HttpResponse(
                    Convert.ToDateTime(dataReader["DateTime_Response"]),
                    Convert.ToInt32(dataReader["Content_Length"]))
                {
                  StatusSend = Convert.ToByte(dataReader["Status_Send"]),
                };
              }
              // Додаємо до загальної колекцiї
              AddRequest(request);
            }

          }
          dataReader.Close();
          cmd.Dispose();

        }
      }
      catch (Exception ex)
      {
        Mediator?.Notify(this, $"SELECT error: {ex.Message}");
      }
      finally
      {
        if (connection.State == System.Data.ConnectionState.Open)
          connection.Close();
      }
      return Requests;
    }

    public void SaveToDB(HttpRequest httpRequest, char typeOper)
    {
      NpgsqlConnection connection = new NpgsqlConnection(ConnStr);
      try
      {
        connection.ConnectionString = ConnStr;
        if (connection.State == System.Data.ConnectionState.Closed)
          connection.Open();
        NpgsqlCommand cmd = connection.CreateCommand();
        cmd.CommandType = System.Data.CommandType.Text;
        string query = string.Empty;
        switch (typeOper)
        {
          case '+':
            query = @"INSERT INTO public.""Http_Request""(" +
                @"""DateTime_Request"", ""Version"", ""Method"", ""Ip_Address_Client"", ""Ip_Address_Server"",""Status"", ""Content_Type_Request"", ""Message"", ""Type_Request"")" +
                @" VALUES ($1, $2, $3, $4, $5, $6, $7, $8, $9)";
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.DateTime, Value = httpRequest.DateTimeRequest });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.String, Value = httpRequest.Version == null ? DBNull.Value : httpRequest.Version });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.String, Value = httpRequest.Method == null ? DBNull.Value : httpRequest.Method });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.String, Value = httpRequest.IpAddressClient });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.String, Value = httpRequest.IpAddressServer });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int32, Value = (int)httpRequest.Status });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.String, Value = httpRequest.ContentTypeRequest == null ? DBNull.Value : httpRequest.ContentTypeRequest });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.String, Value = httpRequest.Message == null ? DBNull.Value : httpRequest.Message });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int16, Value = (int)httpRequest.TypeRequest });
            break;

          case '-':
            query = @"DELETE FROM public.""Http_Request""" +
                    @" WHERE ""Id_Request"" = $1";
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int64, Value = httpRequest.IdRequest });
            break;

          case '=':
            query = @"UPDATE public.""Http_Request""" +
                  @" SET ""Status""= $1" +
                  @" WHERE ""Id_Request""= $2";
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int32, Value = (int)httpRequest.Status });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int64, Value = httpRequest.IdRequest });
            break;

          default:
            break;
        }
        if (string.IsNullOrEmpty(query))
          return;
        cmd.CommandText = query;
        cmd.ExecuteNonQuery();

        // Отримання iдентифiкатора з бази даних для нового об'єкта
        if (typeOper == '+')
        {
          cmd.CommandText = @"SELECT currval('""Http_Request_Seq_Id""')  as id";
          cmd.Parameters.Clear();
          long newIdRequest = Convert.ToInt64(cmd.ExecuteScalar());
          httpRequest.IdRequest = newIdRequest;

          // Http_Response
          if (httpRequest.Response != null)
          {
            cmd.Parameters.Clear();
            query = @"INSERT INTO public.""Http_Response""(" +
                @"""Id_Request"", ""DateTime_Response"", ""Status_Send"", ""Content_Length"")" +
                @" VALUES ($1, $2, $3, $4)";
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int64, Value = httpRequest.IdRequest });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.DateTime, Value = httpRequest.Response.DateTimeResponse });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Byte, Value = httpRequest.Response.StatusSend });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int32, Value = httpRequest.Response.ContentLength });

            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
          }

          // Http_Request_Page
          if (httpRequest is HttpRequestPage requestPage)
          {
            cmd.Parameters.Clear();
            query = @"INSERT INTO public.""Http_Request_Page""(" +
                @"""Id_Request"", ""Path"")" +
                @" VALUES ($1, $2)";
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int64, Value = httpRequest.IdRequest });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.String, Value = requestPage.Path });

            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
          }

          // Http_Request_Stat
          if (httpRequest is HttpRequestStat requestStat)
          {
            cmd.Parameters.Clear();
            query = @"INSERT INTO public.""Http_Request_Stat""(" +
                @"""Id_Request"", ""Date_Beg"", ""Date_End"", ""Cnt_Rows"", ""Key_Authorization"")" +
                @" VALUES ($1, $2, $3, $4, $5)";
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int64, Value = httpRequest.IdRequest });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.DateTimeOffset, Value = requestStat.DateBeg });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.DateTimeOffset, Value = requestStat.DateEnd });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.Int32, Value = requestStat.CntRows });
            cmd.Parameters.Add(new NpgsqlParameter() { DbType = System.Data.DbType.String, Value = requestStat.KeyAuthorization });

            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
          }
        }

        Mediator?.Notify(this, "Данi про запит успiшно записанi до БД");
      }
      catch (Exception ex)
      {
        Mediator?.Notify(this, $"DML error: {ex.Message}");
      }
      finally
      {
        if (connection.State == System.Data.ConnectionState.Open)
          connection.Close();
      }
    }
  }
}
