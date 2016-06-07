using System;
using System.Threading.Tasks;
using ProjectionSystem.Diagnostics;

namespace ProjectionSystem.States.Transitions {
  public class StateTransitionOrchestrator<TItem> : IStateTransitionOrchestrator<TItem> where TItem : IProjectedItem  {
    readonly IProjectionSystem _projectionSystem;
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;
    readonly ITraceLogger _traceLogger;

    public StateTransitionOrchestrator(IProjectionSystem projectionSystem, IStateTransitionGuardFactory stateTransitionGuardFactory, ITraceLogger traceLogger) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      _projectionSystem = projectionSystem;
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
      _traceLogger = traceLogger;
    }

    public async Task TransitionToState(IState<TItem> state) {
      if (state == null) throw new ArgumentNullException(nameof(state));

      var transitionGuard = _stateTransitionGuardFactory.CreateFor(state);
      transitionGuard.StateTransitionAllowed(CurrentState);

      _traceLogger.Verbose($"Entering '{state.Id}' state.");
      await state.BeforeEnter(_projectionSystem);
      CurrentState = state;
      await state.AfterEnter(_projectionSystem);
      _traceLogger.Verbose($"Entered '{state.Id}' state.");
    }

    public async Task TransitionToState(IState state) {
      if (state == null) throw new ArgumentNullException(nameof(state));
      var typedState = state as IState<TItem>;
      if (typedState == null) throw new ArgumentException($"The {GetType().Name} cannot transition to a {state.GetType().Name}.", nameof(state));
      await TransitionToState(typedState);
    }

    public IState<TItem> CurrentState { get; private set; }

    IState IStateTransitionOrchestrator.CurrentState => CurrentState;
  }
}