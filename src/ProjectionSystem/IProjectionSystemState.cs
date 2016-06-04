using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem {
  public interface IProjectionSystemState {
    ProjectionState Id { get; }
    Task Enter(IProjectionSystem projectionSystem);
  }

  public interface IProjectionSystemState<TItem> : IProjectionSystemState
    where TItem : IProjectedItem {
    Task Enter(IProjectionSystem<TItem> projectionSystem);
    Task<IEnumerable<TItem>> GetProjectedData();
  }
}