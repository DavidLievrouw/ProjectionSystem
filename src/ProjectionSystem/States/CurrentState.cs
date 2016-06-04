using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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

    public override Task Enter(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { StateId.Maintaining },
        projectionSystem.State.Id);

      _timer = new Timer(_ => {
        projectionSystem.EnterState(new ExpiredState<TItem>(_projectedData));
        _timer.Dispose();
      }, null, (int)_timeout.TotalMilliseconds, Timeout.Infinite);

      return Task.FromResult(true);
    }

    public override Task<IEnumerable<TItem>> GetProjectedData() {
      return Task.FromResult(_projectedData);
    }
  }
}