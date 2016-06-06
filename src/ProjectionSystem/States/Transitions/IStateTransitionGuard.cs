namespace ProjectionSystem.States {
  public interface IStateTransitionGuard {
    void PreviousStateRequired(IState previousState);
    void StateTransitionAllowed(IState previousState);
  }
}
