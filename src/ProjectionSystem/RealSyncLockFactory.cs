using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem {
  public class RealSyncLockFactory : ISyncLockFactory {
    readonly SemaphoreSlim _semaphore;

    public RealSyncLockFactory(SemaphoreSlim semaphore) {
      if (semaphore == null) throw new ArgumentNullException(nameof(semaphore));
      _semaphore = semaphore;
    }

    public async Task<ISyncLock> Create() {
      var realLock = new RealSyncLock(_semaphore);
      await realLock.Lock().ConfigureAwait(false);
      return realLock;
    }
  }
}