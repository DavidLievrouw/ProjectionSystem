using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public class StateTransitionGuardFactory : IStateTransitionGuardFactory {
    public IStateTransitionGuard CreateFor(IState state, IEnumerable<StateId> allowedPreviousStates) {
      if (state == null) throw new ArgumentNullException(nameof(state));
      if (allowedPreviousStates == null) throw new ArgumentNullException(nameof(allowedPreviousStates));
      return new StateTransitionGuard(state, allowedPreviousStates);
    }
  }
}