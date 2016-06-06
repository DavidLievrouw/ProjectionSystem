using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public class CreatingState<TItem> : ProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly object _syncRoot;
    IEnumerable<TItem> _projectedData;

    public CreatingState(IProjectionDataService<TItem> projectionDataService) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      _projectionDataService = projectionDataService;
      _syncRoot = new object();
    }

    public override StateId Id => StateId.Creating;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { StateId.Uninitialised },
        previousState.Id);

      // Make sure only one refresh action is done at a time
      lock (_syncRoot) {
        _projectionDataService.RefreshProjection();
        _projectedData = _projectionDataService.GetProjection();
      }
      
      projectionSystem.SwitchToCurrentState();
    }

    public override IEnumerable<TItem> GetProjectedData() {
      // Do not allow querying of the data, until creation is finished
      lock (_syncRoot) {
        return _projectedData;
      }
    }
  }
}