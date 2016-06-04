using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem.Samples.Departments {
  public class CurrentState : ProjectionSystemState<Department> {
    readonly IEnumerable<Department> _projectedData;
    readonly TimeSpan _timeout;
    Timer _timer;

    public CurrentState(IEnumerable<Department> projectedData, TimeSpan timeout) {
      if (projectedData == null) throw new ArgumentNullException(nameof(projectedData));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _projectedData = projectedData;
      _timeout = timeout;
    }

    public override ProjectionState Id => ProjectionState.Current;

    public override Task Enter(IProjectionSystem<Department> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { ProjectionState.Maintaining },
        projectionSystem.State.Id);

      _timer = new Timer(_ => {
        projectionSystem.EnterState(new ExpiredState(_projectedData));
        _timer.Dispose();
      }, null, (int)_timeout.TotalMilliseconds, Timeout.Infinite);

      return Task.FromResult(true);
    }

    public override Task<IEnumerable<Department>> GetProjectedData() {
      return Task.FromResult(_projectedData);
    }
  }
}