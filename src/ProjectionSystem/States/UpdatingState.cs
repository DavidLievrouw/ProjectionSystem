using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class UpdatingState<TItem> : ProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly object _syncRoot;
    IEnumerable<TItem> _projectedData;

    public UpdatingState(IProjectionDataService<TItem> projectionDataService) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      _projectionDataService = projectionDataService;
      _syncRoot = new object();
    }

    public override StateId Id => StateId.Updating;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { StateId.Expired },
        previousState.Id);

      // Make sure only one refresh action is done at a time
      lock (_syncRoot) {
        _projectionDataService.RefreshProjection();
        _projectedData = _projectionDataService.GetProjection();
      }
      
      projectionSystem.SwitchToCurrentState();
    }

    public override IEnumerable<TItem> GetProjectedData() {
      // Block until refresh is finished
      lock (_syncRoot) {
        return _projectedData;
      }
    }
  }
}