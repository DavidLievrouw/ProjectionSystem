using System;
using System.Diagnostics;
using DavidLievrouw.Utils;

namespace ProjectionSystem.Diagnostics {
  public class TraceEventTypeAdapter : IAdapter<Severity, TraceEventType> {
    public TraceEventType Adapt(Severity severity) {
      switch (severity) {
        case Severity.Verbose:
          return TraceEventType.Verbose;
        case Severity.Information:
          return TraceEventType.Information;
        case Severity.Warning:
          return TraceEventType.Warning;
        case Severity.Error:
          return TraceEventType.Error;
        case Severity.Critical:
          return TraceEventType.Critical;
        default:
          throw new ArgumentOutOfRangeException(nameof(severity));
      }
    }
  }
}