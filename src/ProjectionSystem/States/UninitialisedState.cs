using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public class UninitialisedState<TItem> : ProjectionSystemState<TItem>
    where TItem : IProjectedItem {

    public override StateId Id => StateId.Uninitialised;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new StateId[0],
        previousState?.Id);
    }

    public override IEnumerable<TItem> GetProjectedData() {
      return null;
    }
  }
}