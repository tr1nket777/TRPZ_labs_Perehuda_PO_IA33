namespace HttpServApp.Builder
{
  /// <summary>
  /// iнтерфейс IBuilder визначає всi можливi кроки з формування HTTP-вiдповiдi
  /// </summary>
  internal interface IBuilder
  {
    /// <summary>
    /// Версiя протоколу
    /// </summary>
    /// <returns></returns>
    public byte[] BuildVersion();

    /// <summary>
    /// Статус виконання запиту
    /// </summary>
    /// <returns></returns>
    public byte[] BuildStatus();

    /// <summary>
    /// Заголовки вiдповiдi
    /// </summary>
    /// <returns></returns>
    public byte[] BuildHeaders();

    /// <summary>
    /// Змiст (тiло) вiдповiдi
    /// </summary>
    /// <returns></returns>
    public byte[] BuildContentBody();

  }
}
