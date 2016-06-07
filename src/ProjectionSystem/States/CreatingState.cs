using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class CreatingState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionSystem<TItem> _projectionSystem;
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    IEnumerable<TItem> _projectedData;

    public CreatingState(IProjectionSystem<TItem> projectionSystem, IProjectionDataService<TItem> projectionDataService, ISyncLockFactory syncLockFactory) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      _projectionSystem = projectionSystem;
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
    }

    public override StateId Id => StateId.Creating;

    public override bool IsTransitionAllowed(StateId? previousState) {
      return previousState.HasValue && previousState.Value == StateId.Uninitialised;
    }

    public override async Task BeforeEnter() {
      // Make sure only one update action is done at a time
      using (await _syncLockFactory.Create().ConfigureAwait(false)) {
        await _projectionDataService.UpdateProjection().ConfigureAwait(false);
        _projectedData = await _projectionDataService.GetProjection().ConfigureAwait(false);
      }
    }

    public override async Task AfterEnter() {
      await _projectionSystem.MarkProjectionAsUpToDate().ConfigureAwait(false);
    }

    public override async Task<IEnumerable<TItem>> GetProjection() {
      // Do not allow querying of the data, until creation is finished
      using (await _syncLockFactory.Create().ConfigureAwait(false)) {
        return _projectedData;
      }
    }
  }
}