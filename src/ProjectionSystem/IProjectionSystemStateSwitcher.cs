namespace ProjectionSystem {
  public interface IProjectionSystemStateSwitcher {
    void SwitchToExpiredState();
    void SwitchToUpdatingState();
    void SwitchToCurrentState();
  }
}
