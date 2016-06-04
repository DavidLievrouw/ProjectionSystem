using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public interface IProjectionSystemState {
    StateId Id { get; }
    Task Enter(IProjectionSystem projectionSystem);
  }

  public interface IProjectionSystemState<TItem> : IProjectionSystemState
    where TItem : IProjectedItem {
    Task Enter(IProjectionSystem<TItem> projectionSystem);
    Task<IEnumerable<TItem>> GetProjectedData();
  }
}