using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class ExpiredState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionSystem<TItem> _projectionSystem;
    IEnumerable<TItem> _projectedData;

    public ExpiredState(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      _projectionSystem = projectionSystem;
    }

    public override StateId Id => StateId.Expired;

    public override bool IsTransitionAllowed(StateId? previousState) {
      return previousState.HasValue && previousState.Value == StateId.Current;
    }

    public override async Task BeforeEnter() {
      _projectedData = await _projectionSystem.State.GetProjection();
    }

    public override Task AfterEnter() {
      return Task.FromResult(true); // Noop
    }

    public override Task<IEnumerable<TItem>> GetProjection() {
      return Task.FromResult(_projectedData);
    }
  }
}