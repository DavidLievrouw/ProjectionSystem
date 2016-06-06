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
        previousState.Id);

      // Keep track of the expired projection
      _projectedData = previousState.GetProjectedData();
    }

    public override IEnumerable<TItem> GetProjectedData() {
      return _projectedData;
    }
  }
}