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
      if (projectionSystem.State.Id == Id) return; // Some other thread has already marked the projection as 'Current'
      StateTransitionGuard(
        new[] { StateId.Updating },
        projectionSystem.State.Id);

      _projectedData = previousState.GetProjectedData();

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