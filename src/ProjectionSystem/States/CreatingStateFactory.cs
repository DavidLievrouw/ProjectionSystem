using System;

namespace ProjectionSystem.States {
  public class CreatingStateFactory<TItem> : IStateFactory<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;

    public CreatingStateFactory(IProjectionDataService<TItem> projectionDataService, ISyncLockFactory syncLockFactory) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
    }

    public IState<TItem> Create(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      return new CreatingState<TItem>(projectionSystem, _projectionDataService, _syncLockFactory);
    }
  }
}