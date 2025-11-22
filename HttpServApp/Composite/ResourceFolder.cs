namespace HttpServApp.Composite
{
  /// <summary>
  /// Клаc ResourceFolder представляє собою контейнер, що має
  /// дочiрнi елементи: вкладенi файли та папки
  /// </summary>
  internal class ResourceFolder: ResourceBase
  {
    protected List<ResourceBase> childrenResource = new List<ResourceBase>();

    /// <summary>
    /// Конструктор класа: генерує дерево вкладених папок та файлiв
    /// </summary>
    /// <param name="resourcePath"></param>
    public ResourceFolder(string resourcePath)
    {
      var directory = new DirectoryInfo(resourcePath);

      if (directory.Exists)
      {
        #region Вкладенi папки
        DirectoryInfo[] dirs = directory.GetDirectories();
        foreach (DirectoryInfo dir in dirs)
        {
          childrenResource.Add(new ResourceFolder(dir.FullName));
        }
        #endregion

        #region Файли
        FileInfo[] files = directory.GetFiles();
        foreach (FileInfo file in files)
        {
          childrenResource.Add(new ResourceFile(directory.FullName, file.Name));
        }
        #endregion
      }
    }

    public override void Add(ResourceBase resource)
    {
      childrenResource.Add(resource);
    }

    public override void Remove(ResourceBase resource)
    {
      childrenResource.Remove(resource);
    }

    /// <summary>
    /// Метод пошуку файлу у дочiрнiх об'єктах
    /// </summary>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public override List<string>? FindResources(string resourceName)
    {
      List<string> resources = new List<string>();
      // Цикл по дочiрнiм об'єктам
      foreach (ResourceBase resource in childrenResource)
      {
        List<string>? findResources = resource.FindResources(resourceName);
        // Якщо знайдено хоча б 1 файл iз заданим iм'ям resourceName, додаємо до колекцiї
        if (findResources != null && findResources.Count > 0)
          resources.AddRange(findResources);
      }

      return resources;
    }
  }
}
