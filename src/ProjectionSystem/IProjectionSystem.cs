using ProjectionSystem.States;

namespace ProjectionSystem {
  public interface IProjectionSystem {
    IState State { get; }
  }

  public interface IProjectionSystem<TItem> : IProjectionSystem, IStateTransitioner
    where TItem : IProjectedItem {
    new IState<TItem> State { get; }
  }
}