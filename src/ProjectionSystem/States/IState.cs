using System.Collections.Generic;

namespace ProjectionSystem.States {
  public interface IState {
    StateId Id { get; }
    void Enter(IProjectionSystem projectionSystem, IState previousState);
  }

  public interface IState<TItem> : IState
    where TItem : IProjectedItem {
    void Enter(IProjectionSystem<TItem> projectionSystem, IState<TItem> previousState);
    IEnumerable<TItem> GetProjection();
  }
}