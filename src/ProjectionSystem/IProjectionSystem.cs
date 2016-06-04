using ProjectionSystem.States;

namespace ProjectionSystem {
  public interface IProjectionSystem {
    IProjectionSystemState State { get; }
    void EnterState(IProjectionSystemState state);
  }

  public interface IProjectionSystem<TItem> : IProjectionSystem
    where TItem : IProjectedItem {
    new IProjectionSystemState<TItem> State { get; }
  }
}