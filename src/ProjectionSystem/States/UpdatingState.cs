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
      if (projectionSystem.State.Id == Id) return; // Some other thread is already updating the projection

      StateTransitionGuard(
        new[] { StateId.Expired },
        projectionSystem.State.Id);

      // Return the old data, as long as the update process is not finished
      _projectedData = previousState.GetProjectedData();

      var forciblyUpdated = false;
      // Make sure only one refresh action is done at a time
      lock (_syncRoot) {
        if (_projectedData == null) {
          forciblyUpdated = true;
          _projectionDataService.RefreshProjection();
          _projectedData = _projectionDataService.GetProjection();
        }
      }

      Task.Run(() => {        
        // Make sure only one refresh action is done at a time
        lock (_syncRoot) {
          if (!forciblyUpdated) {
            _projectionDataService.RefreshProjection();
            _projectedData = _projectionDataService.GetProjection();
          }
        }

        projectionSystem.SwitchToCurrentState();
      });
    }

    public override IEnumerable<TItem> GetProjectedData() {
      // Block until refresh is finished
      lock (_syncRoot) {
        return _projectedData;
      }
    }
  }
}