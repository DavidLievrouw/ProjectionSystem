using System;

namespace ProjectionSystem.Diagnostics {
  public interface ILogEntry {
    DateTimeOffset DateTimeUtc { get; }
    string MachineName { get; }
    string User { get; }
    Version Version { get; }
    int EventId { get; set; }
    Severity Severity { get; set; }

    object Data { get; set; }
    Exception Exception { get; set; }
  }
}