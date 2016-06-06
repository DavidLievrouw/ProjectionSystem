using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ProjectionSystem.States.Transitions;

namespace ProjectionSystem.States {
  public class CurrentState<TItem> : State<TItem> where TItem : IProjectedItem {
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;
    readonly TimeSpan _timeout;
    readonly TaskScheduler _taskScheduler;
    IEnumerable<TItem> _projectedData;

    public CurrentState(
      IStateTransitionGuardFactory stateTransitionGuardFactory,
      TimeSpan timeout,
      TaskScheduler taskScheduler) {
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
      _timeout = timeout;
      _taskScheduler = taskScheduler;
    }

    public override StateId Id => StateId.Current;

    public override async Task Prepare(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var transitionGuard = _stateTransitionGuardFactory.CreateFor(this, new[] { StateId.Creating, StateId.Updating });
      transitionGuard.PreviousStateRequired(projectionSystem.State);
      transitionGuard.StateTransitionAllowed(projectionSystem.State);

      _projectedData = await projectionSystem.State.GetProjection(); // Get the projection that was created or updated
    }

    public override async Task Enter(IProjectionSystem<TItem> projectionSystem) {
      // Expire after the specified amount of time
      await Task.Factory.StartNew(async () => {
        await Task.Delay(_timeout);
        await projectionSystem.TransitionToExpiredState();
      }, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
    }

    public override Task<IEnumerable<TItem>> GetProjection() {
      return Task.FromResult(_projectedData);
    }
  }
}