using System.Threading;

namespace ProjectionSystem {
  public class RealSyncLock : ISyncLock {
    readonly object _toLock;
    readonly bool _lockWasTaken;

    public RealSyncLock(object toLock) {
      _toLock = toLock;
      Monitor.Enter(_toLock, ref _lockWasTaken);
    }

    public void Dispose() {
      if (_lockWasTaken) Monitor.Exit(_toLock);
    }
  }
}