namespace ProjectionSystem.States.Transitions {
  public interface IStateTransitionGuard {
    void StateTransitionAllowed(IState previousState);
  }
}
