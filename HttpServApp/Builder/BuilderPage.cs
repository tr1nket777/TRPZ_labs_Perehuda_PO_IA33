using HttpServApp.Composite;
using HttpServApp.Models;
using HttpServApp.Processing;
using System.Text;

namespace HttpServApp.Builder
{
  /// <summary>
  /// Builder-клас побудови вiдповiдi на запит Web-сторiнки
  /// </summary>
  internal class BuilderPage : IBuilder
  {
    private readonly HttpRequestPage httpRequestPage;
    public BuilderPage(HttpRequest httpRequest)
    {
      httpRequestPage = httpRequest as HttpRequestPage ?? throw new ArgumentNullException(nameof(httpRequestPage));
    }

    public byte[] BuildVersion() => Encoding.UTF8.GetBytes($"HTTP/{httpRequestPage.Version ?? "1.1"} ");
    public byte[] BuildStatus()
    {
      // Якщо задано просто шлях до сторiнки: шлях не мiстить символу "*"
      int indexOfStarSymbol = httpRequestPage.Path.IndexOf("*");
      if (indexOfStarSymbol == -1)
      {
        // Формуємо повний шлях до файлу, враховуючи конфiгурацiю доступу до сховища репозиторiя
        httpRequestPage.Path = Path.Combine(
          Configuration.ResourcePath ?? "C:\\", httpRequestPage.Path);

        // Якщо сторiнка, що запитується, не знайдена в репозиторiї => StatusEnum.NOT_FOUND
        if (!File.Exists(httpRequestPage.Path))
        {
          httpRequestPage.Status = StatusEnum.NOT_FOUND;
          httpRequestPage.Message = 
            $"Файл сторiнки <b>'{httpRequestPage.Path}'</b> не знайдений у репозиторiї";
        }
        // Все добре. Файл знайдений => StatusEnum.OK
        else
        {
          httpRequestPage.Status = StatusEnum.OK;
        }
      }

      // Якщо шлях до файлу мiстить символ '*',
      // це означає, що користувач хоче знайти сторiнку у деревi репозиторiю (включаючи пошук у вкладених папках),
      // починаючи з якогось рiвня
      else
      {
        // Формуємо шлях до файлу: частина строки вiд початку до першого символу "*"
        string folderPath = httpRequestPage.Path.Substring(0, indexOfStarSymbol);
        
        if (!Directory.Exists(
          Path.Combine(Configuration.ResourcePath ?? "C:", folderPath)))
        {
          httpRequestPage.Status = StatusEnum.NOT_FOUND;
          httpRequestPage.Message =
            $"Папка <b>'{folderPath}'</b> не знайдена на серверi";
          return Encoding.UTF8.GetBytes($"{(int)httpRequestPage.Status} {httpRequestPage.Status} ");
        }

        // Формуємо iм'я файлу: частина строки вiд символу "*" до кiнця
        string fileName = httpRequestPage.Path.Substring(indexOfStarSymbol + 1);
        
        // Будуємо дерево папок та файлiв вiдносно folderPath
        ResourceBase resource = new ResourceFolder(
          Path.Combine(Configuration.ResourcePath ?? "C:", folderPath));
        // Шукаємо файли iз заданою назвою
        List<string>? files = resource.FindResources(fileName);
        // Якщо вiдповiдний файл не знайдено або знайдено декiлька файлiв 
        if (files == null || files.Count != 1)
        {
          // Файл вiдсутнiй у деревi репозиторiю => STATUS = NOT_FOUND
          if (files == null || files.Count == 0)
          {
            httpRequestPage.Status = StatusEnum.NOT_FOUND;
            httpRequestPage.Message =
              $"Файл сторiнки <b>'{fileName}'</b> не знайдений у репозиторiї {folderPath} та вкладених папках";
          }

          // Знайдено декiлька файлiв у деревi репозиторiю => STATUS = NOT_ALLOWED
          else
          {
            httpRequestPage.Status = StatusEnum.NOT_ALLOWED;
            httpRequestPage.Message =
              $"Знайдено <b>{files.Count} файла(iв)</b> у репозиторiї {folderPath} з iменем <b>{fileName}:</b>" +
              $"<br/><b>" +
                $"{string.Join("<br/>",
                  files.Select(file => {
                    // Формуємо посилання на знайдені файли
                    string localFile = file.Replace(Configuration.ResourcePath ?? "C:", "");
                    return $"<a href=\"{localFile}\">{localFile}</a>";

                  }).ToArray())}</b>" +
              $"<br/>Введiть повний шлях до потрiбного ресурсу!";
          }
        }
        else
        {
          // Все добре. Файл знайдений в деревi репозиторiю, i вiн єдиний
          httpRequestPage.Status = StatusEnum.OK;
          // Запам'ятовуємо повний шлях до файлу
          httpRequestPage.Path = files[0];
        }
      }

      // Якщо статус обробки запиту сторінки StatusEnum.NOT_FOUND
      // і заданий резервний віддалений сервер, передаємо запит серверу (модель взаємодії peer-to-peer)
      if (httpRequestPage.Status == StatusEnum.NOT_FOUND &&
        !string.IsNullOrEmpty(Configuration.RemoteHost))
      {
        httpRequestPage.Status = StatusEnum.REDIRECT;
        httpRequestPage.Message += $"<br/>Запит перенаправляється на резервний сервер: {Configuration.RemoteHost}:{Configuration.RemotePort}";
      }
      
      return Encoding.UTF8.GetBytes($"{(int)httpRequestPage.Status} {httpRequestPage.Status} ");
    }

    public byte[] BuildHeaders() => Encoding.UTF8.GetBytes(
        $"\nContent-Type:{httpRequestPage.ContentTypeRequest ?? "text/html"};charset=UTF-8;" +
        $"\nConnection: close\n");

    public byte[] BuildContentBody()
    {
      try
      {
        // Якщо статус обробки запиту не дорiвнює StatusEnum.OK,
        // повертаємо у вiдповiдi тест повiдомлення про помилку
        if (httpRequestPage.Status != StatusEnum.OK)
        {
          Console.WriteLine(httpRequestPage.Message);
          httpRequestPage.Message = 
            $"<p style='font-size: 16px; font-family: Verdana; color: #e69138'>" +
            $"{httpRequestPage.Message}" +
            $"</p>";
          httpRequestPage.Response = new HttpResponse(
              DateTime.Now, Encoding.UTF8.GetByteCount(httpRequestPage.Message ?? string.Empty));

          return Encoding.UTF8.GetBytes(
              $"Content-Length:{httpRequestPage.Response?.ContentLength ?? 0}\n\n" +
              $"{httpRequestPage.Message ?? string.Empty}");
        }

        Console.WriteLine($"Повертаємо вмiст файлу: {httpRequestPage.Path}");
        // Повертаємо змiст сторiнки
        byte[] contentData;
        using (BinaryReader br = new BinaryReader(File.Open(httpRequestPage.Path, FileMode.Open)))
        {
          byte[] buffer = new byte[16 * 1024];
          int readBytes;
          using (MemoryStream memoryStream = new MemoryStream())
          {
            while ((readBytes = br.Read(buffer, 0, buffer.Length)) > 0)
              memoryStream.Write(buffer, 0, readBytes);

            contentData = memoryStream.ToArray();
          }
        }

        // Якщо це файл з С#-вставками
        if (Path.GetExtension(httpRequestPage.Path).Equals(".chtml", StringComparison.CurrentCultureIgnoreCase))
        {
          string htmlData = PreProcessing.ParseChtml(
            PreProcessing.GetStringContent(contentData));
          contentData = PreProcessing.GetByteContent(htmlData);
        }

        httpRequestPage.Response = new HttpResponse(
            DateTime.Now, contentData.Length);
        return [
          .. Encoding.UTF8.GetBytes($"Content-Length:{httpRequestPage.Response?.ContentLength ?? 0}\n\n"),
          .. contentData
        ];

      }
      catch (Exception ex)
      {
        httpRequestPage.Status = StatusEnum.BAD_SERVER;
        httpRequestPage.Message = 
          $"<p style='font-size: 16px; font-family: Verdana; color: #e69138'>" +
          $"{ex.Message}" +
          $"</p>";
        httpRequestPage.Response = new HttpResponse(
            DateTime.Now, Encoding.UTF8.GetByteCount(httpRequestPage.Message));

        Console.WriteLine(httpRequestPage.Message);
        return Encoding.UTF8.GetBytes(
          $"Content-Length:{httpRequestPage.Response?.ContentLength ?? 0}\n\n{httpRequestPage.Message}");
      }
    }

  }
}
