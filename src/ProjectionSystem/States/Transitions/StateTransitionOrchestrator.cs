using System;
using System.Threading.Tasks;

namespace ProjectionSystem.States.Transitions {
  public class StateTransitionOrchestrator<TItem> : IStateTransitionOrchestrator<TItem> where TItem : IProjectedItem {
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;

    public StateTransitionOrchestrator(IStateTransitionGuardFactory stateTransitionGuardFactory) {
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
    }

    public async Task TransitionToState(IState<TItem> state) {
      if (state == null) throw new ArgumentNullException(nameof(state));

      var transitionGuard = _stateTransitionGuardFactory.CreateFor(state);
      transitionGuard.StateTransitionAllowed(CurrentState);

      await state.BeforeEnter().ConfigureAwait(false);
      CurrentState = state;
      await state.AfterEnter().ConfigureAwait(false);
    }

    async Task IStateTransitionOrchestrator.TransitionToState(IState state) {
      if (state == null) throw new ArgumentNullException(nameof(state));
      var typedState = state as IState<TItem>;
      if (typedState == null) throw new ArgumentException($"The {GetType().Name} cannot transition to a {state.GetType().Name}.", nameof(state));
      await TransitionToState(typedState).ConfigureAwait(false);
    }

    public IState<TItem> CurrentState { get; private set; }

    IState IStateTransitionOrchestrator.CurrentState => CurrentState;
  }
}