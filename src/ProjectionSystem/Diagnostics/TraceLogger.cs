using System;
using System.Diagnostics;
using DavidLievrouw.Utils;

namespace ProjectionSystem.Diagnostics {
  public class TraceLogger : ITraceLogger {
    readonly TraceSource _traceSource;
    readonly IAdapter<Severity, TraceEventType> _traceEventTypeAdapter;
    readonly IEntryFormatter _entryFormatter;
    readonly ILogEntryFactory _logEntryFactory;

    /// <summary>
    ///   Initializes a new TraceLogger
    /// </summary>
    /// <param name="traceSourceName">
    ///   The name of the corresponding <see cref="TraceSource"/>
    ///   This should match the name specified in 
    ///     web.config/app.config
    ///      |
    ///      -- system.diagnostics
    ///         |
    ///         -- sources 
    ///             |
    ///             -- source name = "..."
    /// </param>
    public TraceLogger(
      string traceSourceName,
      IAdapter<Severity, TraceEventType> traceEventTypeAdapter,
      IEntryFormatter entryFormatter,
      ILogEntryFactory logEntryFactory) {
      if (traceSourceName == null) throw new ArgumentNullException(nameof(traceSourceName));
      if (traceEventTypeAdapter == null) throw new ArgumentNullException(nameof(traceEventTypeAdapter));
      if (entryFormatter == null) throw new ArgumentNullException(nameof(entryFormatter));
      if (logEntryFactory == null) throw new ArgumentNullException(nameof(logEntryFactory));
      _traceSource = new TraceSource(traceSourceName);
      _traceEventTypeAdapter = traceEventTypeAdapter;
      _entryFormatter = entryFormatter;
      _logEntryFactory = logEntryFactory;
    }

    public void Log(ILogEntry entry) {
      if (entry == null) throw new ArgumentNullException(nameof(entry));

      var eventType = _traceEventTypeAdapter.Adapt(entry.Severity);
      var formattedEntry = _entryFormatter.Format(entry);
      _traceSource.TraceData(eventType, entry.EventId, formattedEntry);
    }

    #region Convenience methods

    public void Log(Severity severity, object data) {
      var logEntry = _logEntryFactory.Create();
      logEntry.Severity = severity;
      logEntry.Data = data;
      Log(logEntry);
    }

    public void Log(Severity severity, object data, Exception exception) {
      var logEntry = _logEntryFactory.Create();
      logEntry.Severity = severity;
      logEntry.Data = data;
      logEntry.Exception = exception;
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
  }
}