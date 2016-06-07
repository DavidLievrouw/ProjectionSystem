using System;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class UpdatingStateFactory<TItem> : IStateFactory<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    readonly TaskScheduler _taskScheduler;

    public UpdatingStateFactory(IProjectionDataService<TItem> projectionDataService, ISyncLockFactory syncLockFactory, TaskScheduler taskScheduler) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
      _taskScheduler = taskScheduler;
    }

    public IState<TItem> Create(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      return new UpdatingState<TItem>(projectionSystem, _projectionDataService, _syncLockFactory, _taskScheduler);
    }
  }
}