using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectionSystem.States.Transitions;

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

    public override async Task Enter(IProjectionSystem<TItem> projectionSystem, IState<TItem> previousState) {
      var transitionGuard = _stateTransitionGuardFactory.CreateFor(this, new[] { StateId.Current });
      transitionGuard.PreviousStateRequired(previousState);
      transitionGuard.StateTransitionAllowed(previousState);

      // Keep track of the expired projection
      _projectedData = await previousState.GetProjection();
    }

    public override Task<IEnumerable<TItem>> GetProjection() {
      return Task.FromResult(_projectedData);
    }
  }
}