using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public interface IState {
    StateId Id { get; }
    bool IsTransitionAllowed(StateId? previousState);
    Task BeforeEnter();
    Task AfterEnter();
  }

  public interface IState<TItem> : IState
    where TItem : IProjectedItem {
    Task<IEnumerable<TItem>> GetProjection();
  }
}