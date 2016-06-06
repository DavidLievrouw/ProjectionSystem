using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public interface IState {
    StateId Id { get; }
    Task Enter(IProjectionSystem projectionSystem, IState previousState);
  }

  public interface IState<TItem> : IState
    where TItem : IProjectedItem {
    Task Enter(IProjectionSystem<TItem> projectionSystem, IState<TItem> previousState);
    Task<IEnumerable<TItem>> GetProjection();
  }
}