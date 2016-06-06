using System;
using DavidLievrouw.Utils;
using ProjectionSystem.Diagnostics;

namespace ProjectionSystem.IntegrationTests {
  public class ConsoleTraceLogger : ITraceLogger {
    readonly ISystemClock _systemClock;

    public ConsoleTraceLogger(ISystemClock systemClock) {
      if (systemClock == null) throw new ArgumentNullException(nameof(systemClock));
      _systemClock = systemClock;
    }

    public void Log(ILogEntry entry) {
      WriteEntry(entry);
    }

    #region Convenience methods

    public void Log(Severity severity, object data) {
      var logEntry = new LogEntry(_systemClock) {
        Severity = severity,
        Data = data
      };
      Log(logEntry);
    }

    public void Log(Severity severity, object data, Exception exception) {
      var logEntry = new LogEntry(_systemClock) {
        Severity = severity,
        Data = data,
        Exception = exception
      };
      Log(logEntry);
    }

    public void Verbose(object data) {
      Log(Severity.Verbose, data);
    }

    public void Info(object data) {
      Log(Severity.Information, data);
    }

    public void Warning(object data) {
      Log(Severity.Warning, data);
    }

    public void Error(object data, Exception exception) {
      Log(Severity.Error, data, exception);
    }

    public void Critical(object data, Exception exception) {
      Log(Severity.Critical, data, exception);
    }

    #endregion

    static void WriteEntry(ILogEntry entry) {
      Console.WriteLine($"{entry.DateTimeUtc.ToLocalTime().ToString("HH:mm:ss")} ({entry.Severity}) > {entry.Data}");
    }
  }
}