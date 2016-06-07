using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class CreatingState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    IEnumerable<TItem> _projectedData;

    public CreatingState(IProjectionDataService<TItem> projectionDataService, ISyncLockFactory syncLockFactory) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
    }

    public override StateId Id => StateId.Creating;

    public override bool IsTransitionAllowed(StateId? previousState) {
      return previousState.HasValue && previousState.Value == StateId.Uninitialised;
    }

    public override async Task BeforeEnter(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));

      // Make sure only one update action is done at a time
      using (await _syncLockFactory.Create()) {
        await _projectionDataService.UpdateProjection();
        _projectedData = await _projectionDataService.GetProjection();
      }
    }

    public override async Task AfterEnter(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      await projectionSystem.MarkProjectionAsUpToDate();
    }

    public override async Task<IEnumerable<TItem>> GetProjection() {
      // Do not allow querying of the data, until creation is finished
      using (await _syncLockFactory.Create()) {
        return _projectedData;
      }
    }
  }
}