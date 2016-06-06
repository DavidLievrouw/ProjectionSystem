using System;

namespace ProjectionSystem {
  public class RealSyncLockFactory : ISyncLockFactory {
    public ISyncLock CreateFor(object toLock) {
      if (toLock == null) throw new ArgumentNullException(nameof(toLock));
      return new RealSyncLock(toLock);
    }
  }
}