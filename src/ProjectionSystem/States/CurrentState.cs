using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class CurrentState<TItem> : ProjectionSystemState<TItem> where TItem : IProjectedItem {
    readonly TimeSpan _timeout;
    readonly TaskScheduler _taskScheduler;
    IEnumerable<TItem> _projectedData;

    public CurrentState(TimeSpan timeout, TaskScheduler taskScheduler) {
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _timeout = timeout;
      _taskScheduler = taskScheduler;
    }

    public override StateId Id => StateId.Current;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { StateId.Creating, StateId.Updating },
        previousState.Id);

      _projectedData = previousState.GetProjectedData(); // Get the projection that was created or updated

      // Expire after the specified amount of time
      Task.Factory.StartNew(async () => {
        await Task.Delay(_timeout);
        projectionSystem.SwitchToExpiredState();
      }, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
    }

    public override IEnumerable<TItem> GetProjectedData() {
      return _projectedData;
    }
  }
}