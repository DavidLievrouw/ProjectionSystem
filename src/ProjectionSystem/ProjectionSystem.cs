using System;
using System.Threading.Tasks;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public abstract class ProjectionSystem : IProjectionSystem {
    public IProjectionSystemState State { get; protected set; }
  }

  public class ProjectionSystem<TItem> : ProjectionSystem, IProjectionSystem<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionSystemState<TItem> _currentState;
    readonly IProjectionSystemState<TItem> _expiredState;
    readonly IProjectionSystemState<TItem> _updatingState;
    readonly IProjectionSystemState<TItem> _creatingState;
    readonly ITraceLogger _traceLogger;
    readonly ISyncLockFactory _stateLockFactory;
    readonly object _stateLockObj;

    public ProjectionSystem(
      TimeSpan timeout,
      IProjectionDataService<TItem> projectionDataService,
      ITraceLogger traceLogger,
      ISyncLockFactory stateLockFactory,
      TaskScheduler taskScheduler) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      if (stateLockFactory == null) throw new ArgumentNullException(nameof(stateLockFactory));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _creatingState = new CreatingState<TItem>(projectionDataService, stateLockFactory);
      _currentState = new CurrentState<TItem>(timeout, taskScheduler);
      _expiredState = new ExpiredState<TItem>();
      _updatingState = new UpdatingState<TItem>(projectionDataService, stateLockFactory, taskScheduler);
      _traceLogger = traceLogger;
      _stateLockFactory = stateLockFactory;
      _stateLockObj = new object();

      State = new UninitialisedState<TItem>();
    }

    public new IProjectionSystemState<TItem> State {
      get { return base.State as IProjectionSystemState<TItem>; }
      private set { base.State = value; }
    }

    public void SwitchToExpiredState() {
      using (_stateLockFactory.CreateFor(_stateLockObj)) {
        var previousState = State;
        State = _expiredState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _expiredState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void SwitchToCreatingState() {
      using (_stateLockFactory.CreateFor(_stateLockObj)) {
        var previousState = State;
        State = _creatingState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _creatingState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void SwitchToUpdatingState() {
      using (_stateLockFactory.CreateFor(_stateLockObj)) {
        var previousState = State;
        State = _updatingState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _updatingState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void SwitchToCurrentState() {
      using (_stateLockFactory.CreateFor(_stateLockObj)) {
        var previousState = State;
        State = _currentState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _currentState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }
  }
}