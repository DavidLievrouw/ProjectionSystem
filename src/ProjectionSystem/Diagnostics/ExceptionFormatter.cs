using System;
using System.Text;

namespace ProjectionSystem.Diagnostics {
  public class ExceptionFormatter : IExceptionFormatter {
    const string Indentation = "    ";
    const string Separator = "-------------------------------------------";

    public string Format(Exception exception) {
      if (exception == null) return string.Empty;

      var output = new StringBuilder(string.Empty);
      var level = 0;
      do {
        if (level > 0) AppendLine(ref output, string.Empty, level);
        AppendLine(ref output, (level < 1 ? "---- Exception ----" : "---- Inner exception ----"), level);
        AppendLine(ref output, "Type: " + exception.GetType().FullName, level);

        var exceptionProps = exception.GetType().GetProperties();
        foreach (var exceptionProp in exceptionProps) {
          if (exceptionProp.Name != "InnerException" && exceptionProp.Name != "StackTrace") {
            object objVal = null;
            try {
              objVal = exceptionProp.GetValue(exception, null);
            } catch (System.Reflection.TargetInvocationException) {
              objVal = "{COULD NOT ACCESS PROPERTY}";
            }

            if (objVal == null) {
              AppendLine(ref output, exceptionProp.Name + ": {NULL}", level);
            } else {
              AppendLine(ref output, exceptionProp.Name + ": " + objVal, level);
            }
          }
        }

        if (exception.StackTrace != null) {
          AppendLine(ref output, "Stacktrace:", level);
          AppendLine(ref output, Separator, level);
          AppendLine(ref output, exception.StackTrace, level);
        } else {
          AppendLine(ref output, "Stacktrace: {NULL}", level);
        }

        exception = exception.InnerException;
        level++;
      } while (exception != null);

      return output.ToString();
    }

    static void AppendLine(ref StringBuilder text, string line, int level) {
      if (text == null) throw new ArgumentNullException(nameof(text));
      if (line == null) throw new ArgumentNullException(nameof(line));

      var indentation = new StringBuilder().Insert(0, Indentation, level).ToString();
      line = line.Replace(Environment.NewLine, Environment.NewLine + indentation);

      text.AppendLine(indentation + line);
    }
  }
}