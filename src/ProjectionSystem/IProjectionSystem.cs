using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public interface IProjectionSystem {
    Task InvalidateProjection();
    Task MarkProjectionAsUpToDate();
  }

  public interface IProjectionSystem<TItem> : IProjectionSystem
    where TItem : IProjectedItem {
    Task<IEnumerable<TItem>> GetProjection();
    IState<TItem> State { get; }
  }
}