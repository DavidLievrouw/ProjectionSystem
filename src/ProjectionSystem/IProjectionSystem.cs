using ProjectionSystem.States;

namespace ProjectionSystem {
  public interface IProjectionSystem {
    IProjectionSystemState State { get; }
  }

  public interface IProjectionSystem<TItem> : IProjectionSystem, IProjectionSystemStateSwitcher
    where TItem : IProjectedItem {
    new IProjectionSystemState<TItem> State { get; }
  }
}