using System;
using System.Threading.Tasks;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public abstract class ProjectionSystem : IProjectionSystem {
    public IState State { get; protected set; }
  }

  public class ProjectionSystem<TItem> : ProjectionSystem, IProjectionSystem<TItem>
    where TItem : IProjectedItem {
    readonly IState<TItem> _currentState;
    readonly IState<TItem> _expiredState;
    readonly IState<TItem> _updatingState;
    readonly IState<TItem> _creatingState;
    readonly ITraceLogger _traceLogger;
    readonly ISyncLockFactory _stateLockFactory;
    readonly object _stateLockObj;

    public ProjectionSystem(
      TimeSpan timeout,
      IProjectionDataService<TItem> projectionDataService,
      ITraceLogger traceLogger,
      ISyncLockFactory stateLockFactory,
      IStateTransitionGuardFactory transitionGuardFactory,
      TaskScheduler taskScheduler) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      if (stateLockFactory == null) throw new ArgumentNullException(nameof(stateLockFactory));
      if (taskScheduler == null) throw new ArgumentNullException(nameof(taskScheduler));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _creatingState = new CreatingState<TItem>(this, transitionGuardFactory, projectionDataService, stateLockFactory);
      _currentState = new CurrentState<TItem>(this, transitionGuardFactory, timeout, taskScheduler);
      _expiredState = new ExpiredState<TItem>(transitionGuardFactory);
      _updatingState = new UpdatingState<TItem>(this, transitionGuardFactory, projectionDataService, stateLockFactory, taskScheduler);
      _traceLogger = traceLogger;
      _stateLockFactory = stateLockFactory;
      _stateLockObj = new object();

      State = new UninitialisedState<TItem>(transitionGuardFactory);
    }

    public new IState<TItem> State {
      get { return base.State as IState<TItem>; }
      private set { base.State = value; }
    }

    public void TransitionToExpiredState() {
      using (_stateLockFactory.CreateFor(_stateLockObj)) {
        var previousState = State;
        State = _expiredState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _expiredState.Enter(previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void TransitionToCreatingState() {
      using (_stateLockFactory.CreateFor(_stateLockObj)) {
        var previousState = State;
        State = _creatingState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _creatingState.Enter(previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void TransitionToUpdatingState() {
      using (_stateLockFactory.CreateFor(_stateLockObj)) {
        var previousState = State;
        State = _updatingState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _updatingState.Enter(previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void TransitionToCurrentState() {
      using (_stateLockFactory.CreateFor(_stateLockObj)) {
        var previousState = State;
        State = _currentState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _currentState.Enter(previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }
  }
}