using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class MaintainingState<TItem> : ProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly SemaphoreSlim _semaphore;
    readonly TimeSpan _timeout;
    IEnumerable<TItem> _projectedData;

    public MaintainingState(IEnumerable<TItem> oldProjectedData, TimeSpan timeout, IProjectionDataService<TItem> projectionDataService) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _projectedData = oldProjectedData; // Can be null the first time
      _timeout = timeout;
      _projectionDataService = projectionDataService;
      _semaphore = new SemaphoreSlim(1);
    }

    public override StateId Id => StateId.Maintaining;

    public override Task Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      if (projectionSystem.State.Id == Id) return Task.FromResult(true);
      StateTransitionGuard(
        new[] { StateId.Expired },
        projectionSystem.State.Id);

      Task.Run(async () => {
        // Make sure only one refresh action is done at a time
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try {
          await _projectionDataService.RefreshProjection().ConfigureAwait(false);
          _projectedData = await _projectionDataService.GetProjection().ConfigureAwait(false);
        }
        finally {
          _semaphore.Release();
        }

        await projectionSystem
          .EnterState(new CurrentState<TItem>(_projectedData, _timeout))
          .ConfigureAwait(false);
      });

      return Task.FromResult(true);
    }

    public override async Task<IEnumerable<TItem>> GetProjectedData() {
      // Block until refresh is finished (only when no old data is available)
      await _semaphore.WaitAsync().ConfigureAwait(false);
      try {
        return _projectedData;
      } finally {
        _semaphore.Release();
      }
    }
  }
}