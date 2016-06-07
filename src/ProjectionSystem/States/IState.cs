using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public interface IState {
    StateId Id { get; }
    bool IsTransitionAllowed(StateId? previousState);
    Task BeforeEnter(IProjectionSystem projectionSystem);
    Task AfterEnter(IProjectionSystem projectionSystem);
  }

  public interface IState<TItem> : IState
    where TItem : IProjectedItem {
    Task BeforeEnter(IProjectionSystem<TItem> projectionSystem);
    Task AfterEnter(IProjectionSystem<TItem> projectionSystem);
    Task<IEnumerable<TItem>> GetProjection();
  }
}