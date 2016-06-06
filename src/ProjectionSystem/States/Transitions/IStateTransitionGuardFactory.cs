using System.Collections.Generic;

namespace ProjectionSystem.States {
  public interface IStateTransitionGuardFactory {
    IStateTransitionGuard CreateFor(IState state, IEnumerable<StateId> allowedPreviousStates);
  }
}