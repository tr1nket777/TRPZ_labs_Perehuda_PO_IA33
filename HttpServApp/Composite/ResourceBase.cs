namespace HttpServApp.Composite
{
  /// <summary>
  /// Базовий абстрактний клас 
  /// </summary>
  internal abstract class ResourceBase
  {
    /// <summary>
    /// Метод, що дозволяє зрозумiти, чи може компонент мати дочiрнi об'єкти.
    /// </summary>
    /// <returns></returns>
    public virtual bool IsComposite() => true;

    /// <summary>
    /// Додавання дочiрнього ресурсу
    /// </summary>
    /// <param name="resource"></param>
    /// <exception cref="NotImplementedException"></exception>
    public virtual void Add(ResourceBase resource)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Видалення дочiрнього ресурсу
    /// </summary>
    /// <param name="resource"></param>
    /// <exception cref="NotImplementedException"></exception>
    public virtual void Remove(ResourceBase resource)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Абстрактний метод пошуку перелiку ресурсiв за заданим iм'ям resourceName
    /// Реалiзацiя передбачена у класах-нащадках
    /// </summary>
    /// <param name="resourceName"></param>
    /// <returns></returns>
    public abstract List<string>? FindResources(string resourceName);
  }
}