using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class ExpiredState<TItem> : ProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    readonly IEnumerable<TItem> _projectedData;

    public ExpiredState(IEnumerable<TItem> invalidData) {
      _projectedData = invalidData; // Can be null the first time
    }

    public override StateId Id => StateId.Expired;

    public override Task Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      if (projectionSystem.State.Id == Id) Task.FromResult(true);

      StateTransitionGuard(
        new[] { StateId.Current },
        projectionSystem.State.Id);

      return Task.FromResult(true);
    }

    public override Task<IEnumerable<TItem>> GetProjectedData() {
      return Task.FromResult(_projectedData);
    }
  }
}