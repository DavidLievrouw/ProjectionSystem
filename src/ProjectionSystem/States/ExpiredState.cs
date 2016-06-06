using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public class ExpiredState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;
    IEnumerable<TItem> _projectedData;

    public ExpiredState(IStateTransitionGuardFactory stateTransitionGuardFactory) {
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
    }

    public override StateId Id => StateId.Expired;

    public override void Enter(IState<TItem> previousState) {
      var transitionGuard = _stateTransitionGuardFactory.CreateFor(this, new[] { StateId.Current });
      transitionGuard.PreviousStateRequired(previousState);
      transitionGuard.StateTransitionAllowed(previousState);

      // Keep track of the expired projection
      _projectedData = previousState.GetProjectedData();
    }

    public override IEnumerable<TItem> GetProjectedData() {
      return _projectedData;
    }
  }
}