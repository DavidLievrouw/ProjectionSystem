using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.Samples.Departments {
  public class ExpiredState : ProjectionSystemState<Department> {
    readonly IEnumerable<Department> _projectedData;

    public ExpiredState(IEnumerable<Department> invalidData) {
      _projectedData = invalidData; // Can be null the first time
    }

    public override ProjectionState Id => ProjectionState.Expired;

    public override Task Enter(IProjectionSystem<Department> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { ProjectionState.Current },
        projectionSystem.State.Id);
      return Task.FromResult(true);
    }

    public override Task<IEnumerable<Department>> GetProjectedData() {
      return Task.FromResult(_projectedData);
    }
  }
}