using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public class CreatingState<TItem> : ProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    readonly object _createProjectionLockObj;
    IEnumerable<TItem> _projectedData;

    public CreatingState(IProjectionDataService<TItem> projectionDataService, ISyncLockFactory syncLockFactory) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
      _createProjectionLockObj = new object();
    }

    public override StateId Id => StateId.Creating;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { StateId.Uninitialised },
        previousState.Id);

      // Make sure only one refresh action is done at a time
      using (_syncLockFactory.CreateFor(_createProjectionLockObj)) { 
        _projectionDataService.RefreshProjection();
        _projectedData = _projectionDataService.GetProjection();
      }
      
      projectionSystem.SwitchToCurrentState();
    }

    public override IEnumerable<TItem> GetProjectedData() {
      // Do not allow querying of the data, until creation is finished
      using (_syncLockFactory.CreateFor(_createProjectionLockObj)) {
        return _projectedData;
      }
    }
  }
}