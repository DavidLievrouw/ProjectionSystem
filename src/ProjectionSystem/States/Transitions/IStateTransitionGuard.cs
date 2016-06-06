namespace ProjectionSystem.States.Transitions {
  public interface IStateTransitionGuard {
    void PreviousStateRequired(IState previousState);
    void StateTransitionAllowed(IState previousState);
  }
}
