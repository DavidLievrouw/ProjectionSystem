using System;
using System.Collections.Generic;
using System.Threading;

namespace ProjectionSystem.States {
  public class CurrentState<TItem> : ProjectionSystemState<TItem> where TItem : IProjectedItem {
    readonly TimeSpan _timeout;
    Timer _timer;
    IEnumerable<TItem> _projectedData;

    public CurrentState(TimeSpan timeout) {
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _timeout = timeout;
    }

    public override StateId Id => StateId.Current;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { StateId.Creating, StateId.Updating },
        previousState.Id);

      _projectedData = previousState.GetProjectedData(); // Get the projection that was created or updated

      // Expire after the specified amount of time
      _timer = new Timer(_ => {
        projectionSystem.SwitchToExpiredState();
        _timer.Dispose();
      }, null, (int)_timeout.TotalMilliseconds, Timeout.Infinite);
    }

    public override IEnumerable<TItem> GetProjectedData() {
      return _projectedData;
    }
  }
}