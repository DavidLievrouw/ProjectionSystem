using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading; 
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class ValidState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionSystem<TItem> _projectionSystem;
    readonly TimeSpan _timeout;
    readonly ISleeper _sleeper;
    readonly TaskScheduler _taskScheduler;
    IEnumerable<TItem> _projectedData;

    public ValidState(IProjectionSystem<TItem> projectionSystem, TimeSpan timeout, ISleeper sleeper, TaskScheduler taskScheduler) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      if (sleeper == null) throw new ArgumentNullException(nameof(sleeper));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _projectionSystem = projectionSystem;
      _timeout = timeout;
      _sleeper = sleeper;
      _taskScheduler = taskScheduler;
    }

    public override StateId Id => StateId.Valid;

    public override bool IsTransitionAllowed(StateId? previousState) {
      var allowedPreviousStates = new[] { StateId.Creating, StateId.Updating };
      return previousState.HasValue && allowedPreviousStates.Contains(previousState.Value);
    }

    public override async Task BeforeEnter() {
      // Get the projection that was created or updated
      _projectedData = await _projectionSystem.State.GetProjection().ConfigureAwait(false); 
    }

    public override async Task AfterEnter() {
      // Expire after the specified amount of time
      await Task.Factory.StartNew(async () => {
        await _sleeper
          .Sleep(_timeout)
          .ContinueWith(previous => _projectionSystem.InvalidateProjection())
          .ConfigureAwait(false);
      }, CancellationToken.None, TaskCreationOptions.LongRunning, _taskScheduler)
      .ConfigureAwait(false);
    }

    public override Task<IEnumerable<TItem>> GetProjection() {
      return Task.FromResult(_projectedData);
    }
  }
}