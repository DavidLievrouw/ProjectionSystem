using System;
using System.Threading.Tasks;

namespace ProjectionSystem.States.Transitions {
  public class StateTransitionOrchestrator<TItem> : IStateTransitionOrchestrator<TItem> where TItem : IProjectedItem {
    readonly IProjectionSystem _projectionSystem;
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;

    public StateTransitionOrchestrator(IProjectionSystem projectionSystem, IStateTransitionGuardFactory stateTransitionGuardFactory) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      _projectionSystem = projectionSystem;
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
    }

    public async Task TransitionToState(IState<TItem> state) {
      if (state == null) throw new ArgumentNullException(nameof(state));

      var transitionGuard = _stateTransitionGuardFactory.CreateFor(state);
      transitionGuard.StateTransitionAllowed(CurrentState);

      await state.BeforeEnter(_projectionSystem);
      CurrentState = state;
      await state.AfterEnter(_projectionSystem);
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