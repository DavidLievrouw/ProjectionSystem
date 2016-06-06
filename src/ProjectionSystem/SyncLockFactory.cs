using System;

namespace ProjectionSystem {
  public class SyncLockFactory : ISyncLockFactory {
    public ISyncLock CreateFor(object toLock) {
      if (toLock == null) throw new ArgumentNullException(nameof(toLock));
      return new SyncLock(toLock);
    }
  }
}