using System;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class ValidStateFactory<TItem> : IStateFactory<TItem>
    where TItem : IProjectedItem {
    readonly ISleeper _sleeper;
    readonly TimeSpan _timeout;
    readonly TaskScheduler _taskScheduler;

    public ValidStateFactory(TimeSpan timeout, ISleeper sleeper, TaskScheduler taskScheduler) {
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      if (sleeper == null) throw new ArgumentNullException(nameof(sleeper));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      _timeout = timeout;
      _sleeper = sleeper;
      _taskScheduler = taskScheduler;
    }

    public IState<TItem> Create(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      return new ValidState<TItem>(projectionSystem, _timeout, _sleeper, _taskScheduler);
    }
  }
}