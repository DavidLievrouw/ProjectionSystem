using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public class UninitialisedState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;

    public UninitialisedState(IStateTransitionGuardFactory stateTransitionGuardFactory) {
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
    }

    public override StateId Id => StateId.Uninitialised;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IState<TItem> previousState) {
      var transitionGuard = _stateTransitionGuardFactory.CreateFor(this, new StateId[0]);
      transitionGuard.StateTransitionAllowed(previousState);
    }

    public override IEnumerable<TItem> GetProjection() {
      return null;
    }
  }
}