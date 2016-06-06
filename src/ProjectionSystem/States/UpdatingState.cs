using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectionSystem.States.Transitions;

namespace ProjectionSystem.States {
  public class UpdatingState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    readonly TaskScheduler _taskScheduler;
    IEnumerable<TItem> _projectedData;

    public UpdatingState(
      IStateTransitionGuardFactory stateTransitionGuardFactory,
      IProjectionDataService<TItem> projectionDataService,
      ISyncLockFactory syncLockFactory,
      TaskScheduler taskScheduler) {
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
      _taskScheduler = taskScheduler;
    }

    public override StateId Id => StateId.Updating;

    public override async Task Prepare(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var transitionGuard = _stateTransitionGuardFactory.CreateFor(this, new[] { StateId.Expired });
      transitionGuard.PreviousStateRequired(projectionSystem.State);
      transitionGuard.StateTransitionAllowed(projectionSystem.State);
      
      _projectedData = await projectionSystem.State.GetProjection(); // Keep track of the expired projection, so that subscribers can access it during the update
    }

    public override async Task Enter(IProjectionSystem<TItem> projectionSystem) {
      await Task.Factory.StartNew(async () => {
        // Make sure only one update action is done at a time
        using (await _syncLockFactory.Create()) {
          await _projectionDataService.UpdateProjection();
          _projectedData = await _projectionDataService.GetProjection();
        }

        await projectionSystem.TransitionToCurrentState();
      }, CancellationToken.None, TaskCreationOptions.LongRunning, _taskScheduler);
    }

    public override Task<IEnumerable<TItem>> GetProjection() {
      return Task.FromResult(_projectedData);
    }
  }
}