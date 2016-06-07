using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class CurrentState<TItem> : State<TItem>
    where TItem : IProjectedItem {
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

    public override bool IsTransitionAllowed(StateId? previousState) {
      var allowedPreviousStates = new[] { StateId.Creating, StateId.Updating };
      return previousState.HasValue && allowedPreviousStates.Contains(previousState.Value);
    }

    public override async Task BeforeEnter(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      _projectedData = await projectionSystem.State.GetProjection(); // Get the projection that was created or updated
    }

    public override async Task AfterEnter(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));

      // Expire after the specified amount of time
      await Task.Factory.StartNew(async () => {
        await Task.Delay(_timeout);
        await projectionSystem.InvalidateProjection();
      }, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
    }

    public override Task<IEnumerable<TItem>> GetProjection() {
      return Task.FromResult(_projectedData);
    }
  }
}