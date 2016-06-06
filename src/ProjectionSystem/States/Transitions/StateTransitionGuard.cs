using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectionSystem.States.Transitions {
  public class StateTransitionGuard : IStateTransitionGuard {
    readonly IState _state;
    readonly IEnumerable<StateId> _allowedPreviousStates;

    public StateTransitionGuard(IState state, IEnumerable<StateId> allowedPreviousStates) {
      if (state == null) throw new ArgumentNullException(nameof(state));
      if (allowedPreviousStates == null) throw new ArgumentNullException(nameof(allowedPreviousStates));
      _state = state;
      _allowedPreviousStates = allowedPreviousStates;
    }

    public void PreviousStateRequired(IState previousState) {
      if (previousState == null) throw new InvalidOperationException($"Cannot enter state '{_state.Id}' without a previous state.");
    }

    public void StateTransitionAllowed(IState previousState) {
      var invalidTransitionException = new InvalidOperationException($"State '{previousState.Id}' cannot handle a transition to state '{_state.Id}'.");
      if (_allowedPreviousStates == null || !_allowedPreviousStates.Contains(previousState.Id)) throw invalidTransitionException;
    }
  }
}