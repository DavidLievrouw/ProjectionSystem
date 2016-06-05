using System;
using DavidLievrouw.Utils;

namespace ProjectionSystem.Diagnostics {
  public class LogEntryFactory : ILogEntryFactory {
    private readonly ISystemClock _systemClock;

    public LogEntryFactory(ISystemClock systemClock) {
      if (systemClock == null) {
        throw new ArgumentNullException(nameof(systemClock));
      }
      _systemClock = systemClock;
    }

    public ILogEntry Create() {
      return new LogEntry(_systemClock);
    }
  }
}