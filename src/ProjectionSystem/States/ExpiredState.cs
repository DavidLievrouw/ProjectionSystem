using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class ExpiredState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    IEnumerable<TItem> _projectedData;

    public override StateId Id => StateId.Expired;

    public override bool IsTransitionAllowed(StateId? previousState) {
      return previousState.HasValue && previousState.Value == StateId.Current;
    }

    public override async Task Prepare(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      _projectedData = await projectionSystem.State.GetProjection();
    }

    public override Task Enter(IProjectionSystem<TItem> projectionSystem) {
      return Task.FromResult(true); // Noop
    }

    public override Task<IEnumerable<TItem>> GetProjection() {
      return Task.FromResult(_projectedData);
    }
  }
}