using System;
using System.Collections.Generic;
using ProjectionSystem.States.Transitions;

namespace ProjectionSystem.States {
  public class CreatingState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;
    IEnumerable<TItem> _projectedData;

    public CreatingState(
      IStateTransitionGuardFactory stateTransitionGuardFactory,
      IProjectionDataService<TItem> projectionDataService,
      ISyncLockFactory syncLockFactory) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
    }

    public override StateId Id => StateId.Creating;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var transitionGuard = _stateTransitionGuardFactory.CreateFor(this, new[] { StateId.Uninitialised });
      transitionGuard.PreviousStateRequired(previousState);
      transitionGuard.StateTransitionAllowed(previousState);

      // Make sure only one refresh action is done at a time
      using (_syncLockFactory.Create()) { 
        _projectionDataService.RefreshProjection();
        _projectedData = _projectionDataService.GetProjection();
      }

      projectionSystem.TransitionToCurrentState();
    }

    public override IEnumerable<TItem> GetProjection() {
      // Do not allow querying of the data, until creation is finished
      using (_syncLockFactory.Create()) {
        return _projectedData;
      }
    }
  }
}