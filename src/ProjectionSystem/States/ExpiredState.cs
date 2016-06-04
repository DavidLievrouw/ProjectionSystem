using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public class ExpiredState<TItem> : ProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    IEnumerable<TItem> _projectedData;

    public override StateId Id => StateId.Expired;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));

      StateTransitionGuard(
        new[] { StateId.Current },
        projectionSystem.State.Id);

      _projectedData = previousState.GetProjectedData(); // Can be null the first time
    }

    public override IEnumerable<TItem> GetProjectedData() {
      return _projectedData;
    }
  }
}