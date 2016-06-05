using System;
using System.Reflection;
using DavidLievrouw.Utils;

namespace ProjectionSystem.Diagnostics {
  public class LogEntry : ILogEntry {
    public LogEntry(ISystemClock systemClock) {
      var appAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
      DateTimeUtc = systemClock.UtcNow;
      MachineName = Environment.MachineName;
      User = Environment.UserDomainName + "\\" + Environment.UserName;
      Version = appAssembly.GetName().Version;
      EventId = 0;
      Severity = Severity.Verbose;
      Exception = null;
    }

    public DateTimeOffset DateTimeUtc { get; }
    public string MachineName { get; }
    public string User { get; }
    public Version Version { get; }
    public int EventId { get; set; }
    public Severity Severity { get; set; }
    public object Data { get; set; }
    public Exception Exception { get; set; }
  }
}