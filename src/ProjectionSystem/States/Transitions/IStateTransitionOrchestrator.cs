using System.Threading.Tasks;

namespace ProjectionSystem.States.Transitions {
  public interface IStateTransitionOrchestrator {
    Task TransitionToState(IState state);
    IState CurrentState { get; }
  }

  public interface IStateTransitionOrchestrator<TItem> : IStateTransitionOrchestrator where TItem : IProjectedItem {
    Task TransitionToState(IState<TItem> state);
    new IState<TItem> CurrentState { get; }
  }
}
