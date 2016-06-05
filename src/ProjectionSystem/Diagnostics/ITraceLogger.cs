using System;

namespace ProjectionSystem.Diagnostics {
  public interface ITraceLogger {
    void Log(ILogEntry entry);
    void Log(Severity severity, object data);
    void Log(Severity severity, object data, Exception exception);
    void Verbose(object data);
    void Info(object data);
    void Warning(object data);
    void Error(object data, Exception exception);
    void Critical(object data, Exception exception);
  }
}