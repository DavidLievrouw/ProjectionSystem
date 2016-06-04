using System.Collections.Generic;

namespace ProjectionSystem.States {
  public interface IProjectionSystemState {
    StateId Id { get; }
    void Enter(IProjectionSystem projectionSystem, IProjectionSystemState previousState);
  }

  public interface IProjectionSystemState<TItem> : IProjectionSystemState
    where TItem : IProjectedItem {
    void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState);
    IEnumerable<TItem> GetProjectedData();
  }
}