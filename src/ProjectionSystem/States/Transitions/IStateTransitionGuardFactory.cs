namespace ProjectionSystem.States.Transitions {
  public interface IStateTransitionGuardFactory {
    IStateTransitionGuard CreateFor(IState state);
  }
}