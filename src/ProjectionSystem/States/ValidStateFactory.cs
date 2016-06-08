using System;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class ValidStateFactory<TItem> : IStateFactory<TItem>
    where TItem : IProjectedItem {
    readonly IValidStateTimeoutProvider<TItem> _validStateTimeoutProvider;
    readonly ISleeper _sleeper;
    readonly TaskScheduler _taskScheduler;

    public ValidStateFactory(IValidStateTimeoutProvider<TItem> validStateTimeoutProvider, ISleeper sleeper, TaskScheduler taskScheduler) {
      if (validStateTimeoutProvider == null) throw new ArgumentNullException(nameof(validStateTimeoutProvider));
      if (sleeper == null) throw new ArgumentNullException(nameof(sleeper));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      _validStateTimeoutProvider = validStateTimeoutProvider;
      _sleeper = sleeper;
      _taskScheduler = taskScheduler;
    }

    public IState<TItem> Create(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var timeout = _validStateTimeoutProvider.ProvideTimeout();
      return new ValidState<TItem>(projectionSystem, timeout, _sleeper, _taskScheduler);
    }
  }
}