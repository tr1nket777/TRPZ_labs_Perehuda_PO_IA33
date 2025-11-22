using HttpServApp.Models;
using System.Text;

namespace HttpServApp.Builder
{
  /// <summary>
  /// Builder-клас побудови вiдповiдi на запит статистики
  /// </summary>
  internal class BuilderStat : IBuilder
  {
    private readonly HttpRequestStat httpRequestStat;
    private readonly List<HttpRequest>? periodRequests;
    public BuilderStat(HttpRequest httpRequest)
    {
      try
      {
        httpRequestStat = httpRequest as HttpRequestStat ?? throw new ArgumentNullException(nameof(httpRequestStat));
        periodRequests = httpRequestStat.Repository.GetRequestsByPeriod(httpRequestStat.DateBeg, httpRequestStat.DateEnd);
        httpRequestStat.CntRows = periodRequests.Count;
      }
      catch (ArgumentNullException)
      {
        Console.WriteLine("BuilderStat: Object is not HttpRequest");
        throw;
      }
      catch (Exception exc)
      {
        httpRequestStat.Message = $"Вiдповiдь на запит статистики не сформована: {exc.Message}";
      }
    }

    public byte[] BuildVersion() => Encoding.UTF8.GetBytes($"HTTP/{httpRequestStat.Version ?? "1.1"} ");
    public byte[] BuildStatus()
    {
      // Якщо periodRequests не визначено (сталась помилка при виборi даних), то STATUS = BAD_SERVER
      if (periodRequests == null)
      {
        httpRequestStat.Status = StatusEnum.BAD_SERVER;
      }
      // Якщо ключ авторизацiї не спiвпадає, то STATUS = UNAUTHORIZED
      else if (httpRequestStat.KeyAuthorization != Configuration.KeyAuthorization)
      {
        httpRequestStat.Status = StatusEnum.UNAUTHORIZED;
        httpRequestStat.Message = httpRequestStat.Message =
          $"Неправильний ключ авторизацiї, запит статистики не виконано.";
      }
      else
      {
        // Якщо записи (запити) в репозиторiї БД за обраний перiод не знайдено => STATUS = NOT_FOUND
        if (periodRequests.Count == 0)
        {
          httpRequestStat.Status = StatusEnum.NOT_FOUND;
          httpRequestStat.Message =
            $"Данi статистики за перiод " +
            $"<b>з {httpRequestStat.DateBeg:dd.MM.yyyy HH:mm} по {httpRequestStat.DateEnd:dd.MM.yyyy HH:mm} ВIДСУТНI</b>";
        }
        else
          // Якщо знайденi => STATUS = OK
          httpRequestStat.Status = StatusEnum.OK;
      }

      return Encoding.UTF8.GetBytes($"{(int)httpRequestStat.Status} {httpRequestStat.Status} ");
    }

    public byte[] BuildHeaders() => Encoding.UTF8.GetBytes(
        $"\nContent-Type:{httpRequestStat.ContentTypeRequest ?? "text/html"};charset=UTF-8;" +
        $"\nConnection: close\n");

    public byte[] BuildContentBody()
    {
      // Якщо статус обробки запиту не дорiвнює StatusEnum.OK,
      // повертаємо у вiдповiдi тест повiдомлення про помилку
      if (httpRequestStat.Status != StatusEnum.OK)
      {
        Console.WriteLine(httpRequestStat.Message);
        httpRequestStat.Message = 
          $"<p style='font-size: 16px; font-family: Verdana; color: #e69138'>" +
          $"{httpRequestStat.Message}" +
          $"</p>";
        httpRequestStat.Response = new HttpResponse(
            DateTime.Now, Encoding.UTF8.GetByteCount(httpRequestStat.Message ?? string.Empty));

        return Encoding.UTF8.GetBytes(
            $"Content-Length:{httpRequestStat.Response?.ContentLength ?? 0}\n\n" +
            $"{httpRequestStat.Message ?? string.Empty}");
      }

      string header = $"Данi статистики за перiод " +
      $"з {httpRequestStat.DateBeg:dd.MM.yyyy HH:mm} по {httpRequestStat.DateEnd:dd.MM.yyyy HH:mm}";
      Console.WriteLine($"{header} вибранi");

      string tableResponse = "";
      int indexRow = 1;

      httpRequestStat.Repository.Requests.ForEach(req =>
      {
        tableResponse +=
                    $"\t\t\t\t<tr class=\"{GetClassRow(req.TypeRequest)}\">\n" +
                        $"\t\t\t\t\t<th scope=\"row\" >{indexRow++}</th>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">{req.DateTimeRequest:dd.MM.yyyy HH:mm:ss}</td>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">{req.TypeRequest}</td>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">{req.IpAddressServer}</td>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">{req.IpAddressClient}</td>\n" +
                        $"\t\t\t\t\t<th scope=\"col\" " +
                            $"{(req.Status != StatusEnum.OK && req.TypeRequest != TypeRequestEnum.НЕ_ВИЗНАЧЕНО ? " class=\"text-warning\"" : string.Empty)}>" +
                              $"{req.Status}" +
                            $"</th>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">{(req.Response?.StatusSend == 1 ? "Так" : "Нi")}</td>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">{req.ContentTypeRequest}</td>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">{req.Method}</td>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">{req.Version}</td>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">" +
                            $"{(req.TypeRequest == TypeRequestEnum.СТОРІНКА ? ((HttpRequestPage)req).Path : string.Empty)}" +
                        $"</td>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">" +
                            $"{(req.TypeRequest == TypeRequestEnum.СТАТИСТИКА
                            ? ((HttpRequestStat)req).DateBeg.ToString("dd.MM.yyyy HH:mm") + "-" +
                                ((HttpRequestStat)req).DateEnd.ToString("dd.MM.yyyy HH:mm") + ": " +
                                ((HttpRequestStat)req).CntRows + " запит(iв)"
                            : string.Empty)}" +
                        $"</td>\n" +
                        $"\t\t\t\t\t<td scope=\"col\">{req.Message}</td>\n" +
                    "\t\t\t\t</tr>\n";
      });

      string bodyResponse = TemplateBodyResponse(header, tableResponse);
      // Формуємо об'єкт Response
      httpRequestStat.Response = new HttpResponse(
          DateTime.Now, Encoding.UTF8.GetByteCount(bodyResponse));

      return Encoding.UTF8.GetBytes(
        $"Content-Length:{httpRequestStat.Response?.ContentLength ?? 0}\n\n" +
        $"{bodyResponse}");
      

    }

    #region private допомiжнi методи
    // Клас css для рядка таблицi у залежностi вiд статуса запиту
    private static string GetClassRow(TypeRequestEnum typeRequest)
    {
      return typeRequest switch
      {
        TypeRequestEnum.НЕ_ВИЗНАЧЕНО => "text-danger",
        TypeRequestEnum.СТОРІНКА => "text-success",
        _ => "text-primary",
      };
    }

    // Шаблон тiла вихiдного html
    private static string TemplateBodyResponse(string header, string tableResponse) =>

    "<!DOCTYPE html>\n" +
    "<html>\n" +
        "\t<head>\n" +
            "\t\t<title>Данi статистики</title>\n" +
            "\t\t<meta charset=\"utf-8\" />\n" +
            "\t\t<link href=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/css/bootstrap.min.css\" rel=\"stylesheet\"\n" +
            "\t\tintegrity=\"sha384-EVSTQN3/azprG1Anm3QDgpJLIm9Nao0Yz1ztcQTwFspd3yD65VohhpuuCOmLASjC\" crossorigin=\"anonymous\"/>\n" +
            "\t\t<script src=\"https://cdn.jsdelivr.net/npm/bootstrap@5.0.2/dist/js/bootstrap.bundle.min.js\"\n" +
            "\t\tintegrity=\"sha384-MrcW6ZMFYlzcLA8Nl+NtUVF0sA7MsXsP1UyJoMp4YLEuNSfAP+JcXn/tWtIaxVXM\" crossorigin=\"anonymous\"></script>\n" +
            "\t\t<link rel=\"stylesheet\" href=\"https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.2/css/all.min.css\"\n" +
            "\t\tintegrity=\"sha512-z3gLpd7yknf1YoNbCzqRKc4qyor8gaKU1qmn+CShxbuBusANI9QpRohGBreCFkKxLhei6S9CQXFEbbKuqLg0DA==\" crossorigin=\"anonymous\" referrerpolicy=\"no-referrer\"/>\n" +
        "\t</head>\n" +

        "\t<body>\n" +
            "\t<div class=\"container-fluid mt-2\">\n" +
            $"\t\t<p class=\"text-center fw-weight-bold\">{header}</p>\n" +
            "\t\t<table class=\"table table-striped\">\n" +
                "\t\t\t<thead>\n" +
                    "\t\t\t\t<tr>\n" +
                        "\t\t\t\t\t<th scope=\"col\" >#</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Дата/час</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Тип</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Адреса сервера</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Адреса клієнта</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Статус запиту</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Статус вiдпр. вiдп.</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Тип контенту</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Метод</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Версiя</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Web-сторiнка</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Перiод статист.</th>\n" +
                        "\t\t\t\t\t<th scope=\"col\">Повiд. про помилку</th>\n" +
                    "\t\t\t\t</tr>\n" +
                "\t\t\t</thead>\n" +
                "\t\t\t<tbody>\n" +
                tableResponse +
                "\t\t\t</tbody>\n" +
            "\t\t</table>\n" +
            "\t\t</div>\n" +
        "\t</body>\n" +
    "</html>";
    #endregion
  }
}
