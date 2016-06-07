using System;

namespace ProjectionSystem.States.Transitions {
  public class StateTransitionGuardFactory : IStateTransitionGuardFactory {
    public IStateTransitionGuard CreateFor(IState state) {
      if (state == null) throw new ArgumentNullException(nameof(state));
      return new StateTransitionGuard(state);
    }
  }
}