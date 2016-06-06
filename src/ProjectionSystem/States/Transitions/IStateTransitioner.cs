namespace ProjectionSystem {
  public interface IStateTransitioner {
    void TransitionToExpiredState();
    void TransitionToCreatingState();
    void TransitionToUpdatingState();
    void TransitionToCurrentState();
  }
}
