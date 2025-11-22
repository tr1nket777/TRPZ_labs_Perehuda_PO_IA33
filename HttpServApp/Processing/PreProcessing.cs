using System.Text;
using System.Text.RegularExpressions;

namespace HttpServApp.Processing
{
  /// <summary>
  /// Статичний клас попередньої обробки файлiв .chtml (обробка C# вставок)
  /// </summary>
  internal static class PreProcessing
  {

    /// <summary>
    /// Повертає строку UTF-8 з масиву байтiв
    /// </summary>
    /// <param name="bytesContent"></param>
    /// <returns></returns>
    public static string GetStringContent(byte[] bytesContent) =>
       Encoding.UTF8.GetString(bytesContent);

    /// <summary>
    /// Повертає масив байтiв зi строки в форматы UTF-8
    /// </summary>
    /// <param name="strContent"></param>
    /// <returns></returns>
    public static byte[] GetByteContent(string strContent) =>
       Encoding.UTF8.GetBytes(strContent);

    /// <summary>
    /// Основний метод для парсингу простих C# вставок
    /// </summary>
    /// <param name="content"></param>
    /// <returns></returns>
    public static string ParseChtml(string content)
    {
      // Словник для змiнних
      Dictionary<string, object> variables = new Dictionary<string, object>();

      #region Обробка конструкцiї @foreach (var @variable in new { item1, item2, ..., itemN }) { body }
      content = Regex.Replace(content, @"@foreach\s*\(var ([a-zA-Z0-9_]+)\s+in\s+new\s*{\s*([^}]+)}\)\s*{([^}]+)}", match =>
      {
        
        string result = string.Empty;
        if (match.Groups.Count > 3)
        {
          // Парсимо вираз
          // Змiнна колекцiї
          string iterVariable = match.Groups[1].Value.Trim();
          // Сама колекцiя елементiв, роздiлених комою
          string collectionExpression = match.Groups[2].Value.Trim();
          // Тiло цикла
          string body = match.Groups[3].Value.Trim();

          // Отримуємо список елементiв
          foreach (var itemValue in GetCollectionFromExpression(collectionExpression))
          {
            // Замiнюємо @iterVariable на значення елементу
            result += body.Replace("@" + iterVariable, itemValue.Replace("\"", ""));
          }
        }
        return result;
      });
      #endregion

      #region Вiдображення значення поточного часу: @@Now
      content = Regex.Replace(content, @"@@Now", match =>
      {
        if (match.Groups.Count > 0)
        {
          return DateTime.Now.ToString("HH:mm:ss");
        }
        return match.Value;
      });
      #endregion

      #region Вiдображення значення поточної дати: @@Today 
      content = Regex.Replace(content, @"@@Today", match =>
      {
        if (match.Groups.Count > 0)
        {
          return DateTime.Today.ToString("dd.MM.yyyy");
        }
        return match.Value;
      });
      #endregion

      #region Обробка iнiцiалiзацiї змiнних: @var variableName=variableValue;
      content = Regex.Replace(content, @"@var\s+([a-zA-Z0-9_]+)\s*=\s*""?([^""]+)""?;", match =>
      {
        if (match.Groups.Count > 2)
        {
          var variableName = match.Groups[1].Value;
          var variableValue = match.Groups[2].Value;

          // Якщо у словнику змiнних така змiнна вже є, заново її iнiцiалiзуємо
          if (variables.ContainsKey(variableName))
            variables[variableName] = variableValue;
          // Якщо такої змiнної немає, додаємо її у словник
          else
            variables.Add(variableName, variableValue);
        }

        // Повертаємо пустий рядок, тому що в html операцiя iнiцiалiзацiї змiнної не вiдображається
        return string.Empty;
      });
      #endregion

      #region Обробка вiдображення значень змiнних: @variableName або реалiзацiя iнкременту/декременту @++variableName @--variableName
      content = Regex.Replace(content, @"@(\+{2}|\-{2})?([a-zA-Z0-9_]+)", match =>
      {
        if (match.Groups.Count > 2)
        {
          var oper = match.Groups[1].Value;
          var variableName = match.Groups[2].Value;
          // Перевiряємо наявнiсть змiнної у словнику
          if (variables.ContainsKey(variableName))
          {
            // Якщо задана операцiя iнкременту/декременту
            if (oper != string.Empty && int.TryParse(variables[variableName].ToString(), out int intValue))
              variables[variableName] = oper == "++" ? ++intValue : --intValue;
            // Виводимо значення змiнної
            return variables[variableName].ToString();
          }
        }

        // Якщо змiнна не знайдена у словнику, виведемо в html текст як вiн є
        return match.Value;
      });
      #endregion

      #region Обробка конструкцiї @if (condition) { ... } else { ... }
      content = Regex.Replace(content, @"@if\s*\(([^}]+)\)\s*{([^}]+)}\s*else\s*{([^}]+)}", match =>
      {
        // Перевiрка умови if
        string condition = match.Groups[1].Value.Trim();
        string trueBody = match.Groups[2].Value.Trim();
        string falseBody = match.Groups[3].Value.Trim();

        // Виконуємо перевiрку умови
        bool conditionResult = EvaluateCondition(condition);
        return conditionResult ? trueBody : falseBody;
      });
      #endregion

      return content;
    }

    /// <summary>
    /// Метод для отримання значень для конструкцiї @foreach
    /// </summary>
    /// <param name="expression"></param>
    /// <returns></returns>
    private static List<string> GetCollectionFromExpression(string expression) =>
      expression.Split(',').ToList();

    /// <summary>
    ///  Метод для перевiрки умови @if (condition)
    /// </summary>
    /// <param name="condition"></param>
    /// <returns></returns>
    private static bool EvaluateCondition(string condition)
    {
      string pattern = @"""?([^""]+)""?\s*([<>!=]{1,2})\s*""?([^""]+)""?";
      Match match = Regex.Match(condition, pattern,
          RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace);
      if (match != Match.Empty && match.Groups.Count > 2)
      {
        string val1 = match.Groups[1].Value;
        string val2 = match.Groups[3].Value;
        string compareSign = match.Groups[2].Value;
        bool isInteger = int.TryParse(val1, out int iVal1);
        isInteger = int.TryParse(val2, out int iVal2) && isInteger;
        switch (compareSign)
        {
          case "==":
            return val1 == val2;
          case "!=":
            return val1 != val2;
          case ">":
            if (isInteger)
              return iVal1 > iVal2;
            return false;
          case "<":
            if (isInteger)
              return iVal1 < iVal2;
            break;
          case ">=":
            if (isInteger)
              return iVal1 >= iVal2;
            break;
          case "<=":
            if (isInteger)
              return iVal1 <= iVal2;
            break;
          default:
            return false;
        }
      }

      return false;
    }
  }
}
