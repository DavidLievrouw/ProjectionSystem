using System;

namespace ProjectionSystem {
  public class RealSyncLockFactory : ISyncLockFactory {
    readonly object _toLock;

    public RealSyncLockFactory(object toLock) {
      if (toLock == null) throw new ArgumentNullException(nameof(toLock));
      _toLock = toLock;
    }

    public ISyncLock Create() {
      return new RealSyncLock(_toLock);
    }
  }
}