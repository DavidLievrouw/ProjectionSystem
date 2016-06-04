using System.Threading.Tasks;

namespace ProjectionSystem {
  public interface IProjectionSystem {
    IProjectionSystemState State { get; }
    Task EnterState(IProjectionSystemState state);
  }

  public interface IProjectionSystem<TItem> : IProjectionSystem
    where TItem : IProjectedItem {
    new IProjectionSystemState<TItem> State { get; }
  }
}