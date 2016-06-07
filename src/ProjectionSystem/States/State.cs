using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public abstract class State : IState {
    public abstract bool IsTransitionAllowed(StateId? previousState);
    public abstract Task BeforeEnter();
    public abstract Task AfterEnter();
    public abstract StateId Id { get; }
  }

  public abstract class State<TItem> : State, IState<TItem>
    where TItem : IProjectedItem {
    public abstract Task<IEnumerable<TItem>> GetProjection();
  }
}