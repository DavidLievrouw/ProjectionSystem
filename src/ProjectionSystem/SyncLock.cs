using System.Threading;

namespace ProjectionSystem {
  public class SyncLock : ISyncLock {
    readonly object _toLock;
    readonly bool _lockWasTaken;

    public SyncLock(object toLock) {
      _toLock = toLock;
      Monitor.Enter(_toLock, ref _lockWasTaken);
    }

    public virtual void Dispose() {
      if (_lockWasTaken) Monitor.Exit(_toLock);
    }
  }
}