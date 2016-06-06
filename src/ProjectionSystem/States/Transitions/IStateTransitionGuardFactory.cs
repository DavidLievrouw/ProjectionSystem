using System.Collections.Generic;

namespace ProjectionSystem.States.Transitions {
  public interface IStateTransitionGuardFactory {
    IStateTransitionGuard CreateFor(IState state, IEnumerable<StateId> allowedPreviousStates);
  }
}