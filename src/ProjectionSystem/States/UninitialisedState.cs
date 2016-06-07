using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class UninitialisedState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    public override StateId Id => StateId.Uninitialised;

    public override bool IsTransitionAllowed(StateId? previousState) {
      return !previousState.HasValue;
    }

    public override Task BeforeEnter(IProjectionSystem<TItem> projectionSystem) {
      return Task.FromResult(true); // Noop
    }

    public override Task AfterEnter(IProjectionSystem<TItem> projectionSystem) {
      return Task.FromResult(true); // Noop
    }

    public override Task<IEnumerable<TItem>> GetProjection() {
      return Task.FromResult((IEnumerable<TItem>)null);
    }
  }
}