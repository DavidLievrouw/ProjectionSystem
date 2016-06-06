using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class UpdatingState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IStateTransitioner _stateTransitioner;
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    readonly TaskScheduler _taskScheduler;
    readonly object _updateProjectionLockObj;
    IEnumerable<TItem> _projectedData;

    public UpdatingState(
      IStateTransitioner stateTransitioner,
      IStateTransitionGuardFactory stateTransitionGuardFactory,
      IProjectionDataService<TItem> projectionDataService,
      ISyncLockFactory syncLockFactory,
      TaskScheduler taskScheduler) {
      if (stateTransitioner == null) throw new ArgumentNullException(nameof(stateTransitioner));
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      _stateTransitioner = stateTransitioner;
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
      _taskScheduler = taskScheduler;
      _updateProjectionLockObj = new object();
    }

    public override StateId Id => StateId.Updating;

    public override void Enter(IState<TItem> previousState) {
      var transitionGuard = _stateTransitionGuardFactory.CreateFor(this, new[] { StateId.Expired });
      transitionGuard.PreviousStateRequired(previousState);
      transitionGuard.StateTransitionAllowed(previousState);
      
      _projectedData = previousState.GetProjectedData(); // Keep track of the expired projection, so that subscribers can access it during the update

      Task.Factory.StartNew(() => {
        // Make sure only one update action is done at a time
        using (_syncLockFactory.CreateFor(_updateProjectionLockObj)) {
          _projectionDataService.RefreshProjection();
          _projectedData = _projectionDataService.GetProjection();
        }

        _stateTransitioner.TransitionToCurrentState();
      }, CancellationToken.None, TaskCreationOptions.LongRunning, _taskScheduler);
    }

    public override IEnumerable<TItem> GetProjectedData() {
      return _projectedData;
    }
  }
}