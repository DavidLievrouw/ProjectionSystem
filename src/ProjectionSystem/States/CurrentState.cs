using System;
using System.Collections.Generic;
using System.Threading;

namespace ProjectionSystem.States {
  public class CurrentState<TItem> : ProjectionSystemState<TItem> where TItem : IProjectedItem {
    readonly IEnumerable<TItem> _projectedData;
    readonly TimeSpan _timeout;
    Timer _timer;

    public CurrentState(IEnumerable<TItem> projectedData, TimeSpan timeout) {
      if (projectedData == null) throw new ArgumentNullException(nameof(projectedData));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _projectedData = projectedData;
      _timeout = timeout;
    }

    public override StateId Id => StateId.Current;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { StateId.Maintaining },
        projectionSystem.State.Id);

      _timer = new Timer(_ => {
        projectionSystem.EnterState(new ExpiredState<TItem>(_projectedData));
        _timer.Dispose();
      }, null, (int)_timeout.TotalMilliseconds, Timeout.Infinite);
    }

    public override IEnumerable<TItem> GetProjectedData() {
      return _projectedData;
    }
  }
}