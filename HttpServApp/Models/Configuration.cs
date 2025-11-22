using System.Configuration;

namespace HttpServApp.Models
{
  internal static class Configuration
  {
    // Порт Http-сервера
    public static int Port { get; set; } = Convert.ToInt32(ConfigurationManager.AppSettings["port"]);
    // Кiлькiсть вхiдних пiдключень у черзi на обробку
    public static int BackLog { get; set; } = Convert.ToInt32(ConfigurationManager.AppSettings["back_log"]);

    public static bool MultiThread { get; set; } = Convert.ToBoolean(ConfigurationManager.AppSettings["multi_thread"]);
    // Шлях для доступу до репозиторiю Web-сторiнок
    public static string? ResourcePath { get; set; } = Convert.ToString(ConfigurationManager.AppSettings["resource_path"]);
    // Строка пiдключення до БД
    public static string? DBConnStr { get; set; } = Convert.ToString(ConfigurationManager.AppSettings["db_conn_str"]);

    // Ключ доступа адмiнiстратора до даних статистики
    public static string? KeyAuthorization { get; } = Convert.ToString(ConfigurationManager.AppSettings["key_authorization"]);

    // IP-адреса або ім'я віддаленого резервного серверу, до якого виконується запит сторінки, якщо вона не знайдена на поточному сервері
    public static string? RemoteHost { get; } = Convert.ToString(ConfigurationManager.AppSettings["remote_host"]);

    // Порт віддаленого резервного серверу, до якого виконується запит сторінки, якщо вона не знайдена на поточному сервері
    public static int? RemotePort { get; } = Convert.ToInt32(ConfigurationManager.AppSettings["remote_port"]);

  }
}
