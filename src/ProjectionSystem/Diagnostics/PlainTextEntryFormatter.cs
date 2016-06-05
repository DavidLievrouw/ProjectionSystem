using System;

namespace ProjectionSystem.Diagnostics {
  public class PlainTextEntryFormatter : IEntryFormatter {
    readonly IExceptionFormatter _exceptionFormatter;

    public PlainTextEntryFormatter(IExceptionFormatter exceptionFormatter) {
      if (exceptionFormatter == null) throw new ArgumentNullException(nameof(exceptionFormatter));
      _exceptionFormatter = exceptionFormatter;
    }

    public string Format(ILogEntry entry) {
      if (entry == null) throw new ArgumentNullException(nameof(entry));

      var exceptionString = string.Empty;
      if (entry.Exception != null) {
        exceptionString = $@"

{_exceptionFormatter.Format(entry.Exception)}";
      }

      return
        $@"DateTime (UTC): {entry.DateTimeUtc.ToString("MMM dd, HH:mm:ss")}
EventId: {entry.EventId.ToString()}
Severity: {entry.Severity.ToString()}
Machine: {entry.MachineName}
User: {entry.User}
AppVersion: {entry.Version.ToString()}

---- Data ----
{entry.Data?.ToString() ?? "[NULL]"}{exceptionString}
===========================================================================
";
    }
  }
}