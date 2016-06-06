using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem {
  public class RealSyncLock : ISyncLock {
    readonly SemaphoreSlim _semaphore;

    public RealSyncLock(SemaphoreSlim semaphore) {
      if (semaphore == null) throw new ArgumentNullException(nameof(semaphore));
      _semaphore = semaphore;
    }

    public async Task Lock() {
      await _semaphore.WaitAsync();
    }

    public void Dispose() {
      _semaphore.Release();
    }
  }
}