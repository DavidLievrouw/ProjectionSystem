using System;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class CurrentStateFactory<TItem> : IStateFactory<TItem>
    where TItem : IProjectedItem {
    readonly TimeSpan _timeout;
    readonly TaskScheduler _taskScheduler;

    public CurrentStateFactory(TimeSpan timeout, TaskScheduler taskScheduler) {
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      _timeout = timeout;
      _taskScheduler = taskScheduler;
    }

    public IState<TItem> Create(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      return new CurrentState<TItem>(projectionSystem, _timeout, _taskScheduler);
    }
  }
}