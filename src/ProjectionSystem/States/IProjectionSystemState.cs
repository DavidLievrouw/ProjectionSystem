using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public interface IProjectionSystemState {
    StateId Id { get; }
    Task Enter(IProjectionSystem projectionSystem, IProjectionSystemState previousState);
  }

  public interface IProjectionSystemState<TItem> : IProjectionSystemState
    where TItem : IProjectedItem {
    Task Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState);
    Task<IEnumerable<TItem>> GetProjectedData();
  }
}