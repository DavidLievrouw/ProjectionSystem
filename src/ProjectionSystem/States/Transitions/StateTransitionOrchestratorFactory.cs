using System;
using ProjectionSystem.Diagnostics;

namespace ProjectionSystem.States.Transitions {
  public class StateTransitionOrchestratorFactory<TItem> : IStateTransitionOrchestratorFactory<TItem>
    where TItem : IProjectedItem {
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;
    readonly ITraceLogger _traceLogger;

    public StateTransitionOrchestratorFactory(IStateTransitionGuardFactory stateTransitionGuardFactory, ITraceLogger traceLogger) {
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
      _traceLogger = traceLogger;
    }

    public IStateTransitionOrchestrator<TItem> CreateFor(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      return new StateTransitionOrchestrator<TItem>(projectionSystem, _stateTransitionGuardFactory, _traceLogger);
    }
  }
}