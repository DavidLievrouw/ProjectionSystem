using System;
using System.Threading.Tasks;
using ProjectionSystem.Diagnostics;

namespace ProjectionSystem.States.Transitions {
  public class LoggingTransitionOrchestrator<TItem> : IStateTransitionOrchestrator<TItem>
    where TItem : IProjectedItem {
    readonly IStateTransitionOrchestrator<TItem> _inner;
    readonly ITraceLogger _traceLogger;

    public LoggingTransitionOrchestrator(IStateTransitionOrchestrator<TItem> inner, ITraceLogger traceLogger) {
      if (inner == null) throw new ArgumentNullException(nameof(inner));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      _inner = inner;
      _traceLogger = traceLogger;
    }

    public async Task TransitionToState(IState state) {
      _traceLogger.Verbose($"Entering '{state.Id}' state.");
      await _inner.TransitionToState(state);
      _traceLogger.Verbose($"Entered '{state.Id}' state.");
    }

    public async Task TransitionToState(IState<TItem> state) {
      _traceLogger.Verbose($"Entering '{state.Id}' state.");
      await _inner.TransitionToState(state);
      _traceLogger.Verbose($"Entered '{state.Id}' state.");
    }
    
    IState IStateTransitionOrchestrator.CurrentState => CurrentState;
    public IState<TItem> CurrentState => _inner.CurrentState;
  }
}