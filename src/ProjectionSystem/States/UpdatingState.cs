using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class UpdatingState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionSystem<TItem> _projectionSystem;
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    readonly TaskScheduler _taskScheduler;
    IEnumerable<TItem> _projectedData;

    public UpdatingState(IProjectionSystem<TItem> projectionSystem, IProjectionDataService<TItem> projectionDataService, ISyncLockFactory syncLockFactory, TaskScheduler taskScheduler) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      _projectionSystem = projectionSystem;
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
      _taskScheduler = taskScheduler;
    }

    public override StateId Id => StateId.Updating;

    public override bool IsTransitionAllowed(StateId? previousState) {
      return previousState.HasValue && previousState.Value == StateId.Expired;
    }

    public override async Task BeforeEnter() {
      _projectedData = await _projectionSystem.State.GetProjection(); // Keep track of the expired projection, so that subscribers can access it during the update
    }

    public override async Task AfterEnter() {
      // Update asynchronously so that other threads receive the previous projection and don't wait for the update
      await Task.Factory.StartNew(async () => {
        // Make sure only one update action is done at a time
        using (await _syncLockFactory.Create()) {
          await _projectionDataService.UpdateProjection();
          _projectedData = await _projectionDataService.GetProjection();
        }

        await _projectionSystem.MarkProjectionAsUpToDate();
      }, CancellationToken.None, TaskCreationOptions.LongRunning, _taskScheduler);
    }

    public override Task<IEnumerable<TItem>> GetProjection() {
      return Task.FromResult(_projectedData);
    }
  }
}