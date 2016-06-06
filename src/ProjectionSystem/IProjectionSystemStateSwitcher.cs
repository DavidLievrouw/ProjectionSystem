namespace ProjectionSystem {
  public interface IProjectionSystemStateSwitcher {
    void SwitchToExpiredState();
    void SwitchToCreatingState();
    void SwitchToUpdatingState();
    void SwitchToCurrentState();
  }
}
