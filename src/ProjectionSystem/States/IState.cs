using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public interface IState {
    StateId Id { get; }
    bool IsTransitionAllowed(StateId? previousState);
    Task Prepare(IProjectionSystem projectionSystem);
    Task Enter(IProjectionSystem projectionSystem);
  }

  public interface IState<TItem> : IState
    where TItem : IProjectedItem {
    Task Prepare(IProjectionSystem<TItem> projectionSystem);
    Task Enter(IProjectionSystem<TItem> projectionSystem);
    Task<IEnumerable<TItem>> GetProjection();
  }
}