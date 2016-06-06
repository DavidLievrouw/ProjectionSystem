using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class UpdatingState<TItem> : ProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    readonly TaskScheduler _taskScheduler;
    readonly object _updateProjectionLockObj;
    IEnumerable<TItem> _projectedData;

    public UpdatingState(IProjectionDataService<TItem> projectionDataService, ISyncLockFactory syncLockFactory, TaskScheduler taskScheduler) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
      _taskScheduler = taskScheduler;
      _updateProjectionLockObj = new object();
    }

    public override StateId Id => StateId.Updating;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { StateId.Expired },
        previousState.Id);

      _projectedData = previousState.GetProjectedData(); // Keep track of the expired projection, so that subscribers can access it during the update

      Task.Factory.StartNew(() => {
        // Make sure only one update action is done at a time
        using (_syncLockFactory.CreateFor(_updateProjectionLockObj)) {
          _projectionDataService.RefreshProjection();
          _projectedData = _projectionDataService.GetProjection();
        }

        projectionSystem.SwitchToCurrentState();
      }, CancellationToken.None, TaskCreationOptions.LongRunning, _taskScheduler);
    }

    public override IEnumerable<TItem> GetProjectedData() {
      return _projectedData;
    }
  }
}