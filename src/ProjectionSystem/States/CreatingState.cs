using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public class CreatingState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionSystem<TItem> _projectionSystem;
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;
    readonly object _createProjectionLockObj;
    IEnumerable<TItem> _projectedData;

    public CreatingState(IProjectionSystem<TItem> projectionSystem, IStateTransitionGuardFactory stateTransitionGuardFactory, IProjectionDataService<TItem> projectionDataService, ISyncLockFactory syncLockFactory) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      _projectionSystem = projectionSystem;
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
      _createProjectionLockObj = new object();
    }

    public override StateId Id => StateId.Creating;

    public override void Enter(IState<TItem> previousState) {
      var transitionGuard = _stateTransitionGuardFactory.CreateFor(this, new[] { StateId.Uninitialised });
      transitionGuard.PreviousStateRequired(previousState);
      transitionGuard.StateTransitionAllowed(previousState);

      // Make sure only one refresh action is done at a time
      using (_syncLockFactory.CreateFor(_createProjectionLockObj)) { 
        _projectionDataService.RefreshProjection();
        _projectedData = _projectionDataService.GetProjection();
      }
      
      _projectionSystem.TransitionToCurrentState();
    }

    public override IEnumerable<TItem> GetProjectedData() {
      // Do not allow querying of the data, until creation is finished
      using (_syncLockFactory.CreateFor(_createProjectionLockObj)) {
        return _projectedData;
      }
    }
  }
}