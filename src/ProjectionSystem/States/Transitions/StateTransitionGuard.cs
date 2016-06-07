using System;

namespace ProjectionSystem.States.Transitions {
  public class StateTransitionGuard : IStateTransitionGuard {
    readonly IState _state;

    public StateTransitionGuard(IState state) {
      if (state == null) throw new ArgumentNullException(nameof(state));
      _state = state;
    }

    public void StateTransitionAllowed(IState previousState) {
      if (!_state.IsTransitionAllowed(previousState?.Id))
        throw new InvalidStateTransitionException(previousState, _state, $"State '{previousState?.Id.ToString() ?? "[NULL]"}' is not allowed to transition to state '{_state.Id}'.");
    }
  }
}