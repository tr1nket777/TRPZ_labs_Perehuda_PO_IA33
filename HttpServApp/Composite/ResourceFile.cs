namespace HttpServApp.Composite
{
  /// <summary>
  /// Клаc ResourceFile представляє собою файл - кiнцевий вузол дерева
  /// </summary>
  internal class ResourceFile: ResourceBase
  {
    // Шлях до файлу
    private readonly string filePath;
    // iм'я файлу
    private readonly string fileName;
    public ResourceFile(string filePath, string fileName) 
    {
      this.filePath = filePath;
      this.fileName = fileName;
    }
    public override bool IsComposite() => false;

    /// <summary>
    /// Метод пошуку файлу у дочiрнiх об'єктах
    /// </summary>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public override List<string>? FindResources(string resourceName)
    {
      // Якщо iм'я файлу спiвпадає iз заданим resourceName (без врахування регiстру),
      // формуємо колекцiю знайдених елементiв у складi повного шлiху до файла
      if (resourceName.Equals(fileName, StringComparison.CurrentCultureIgnoreCase))
        return new List<string>() { Path.Combine(filePath, fileName) };
      return null;
    }
  }
}
