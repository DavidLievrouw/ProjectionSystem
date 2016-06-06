using System.Collections.Generic;

namespace ProjectionSystem.States {
  public interface IState {
    StateId Id { get; }
    void Enter(IState previousState);
  }

  public interface IState<TItem> : IState
    where TItem : IProjectedItem {
    void Enter(IState<TItem> previousState);
    IEnumerable<TItem> GetProjectedData();
  }
}