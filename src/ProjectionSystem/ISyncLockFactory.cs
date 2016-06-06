namespace ProjectionSystem {
  public interface ISyncLockFactory {
    ISyncLock CreateFor(object toLock);
  }
}