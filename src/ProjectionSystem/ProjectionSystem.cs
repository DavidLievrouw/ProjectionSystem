using System;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public abstract class ProjectionSystem : IProjectionSystem {
    public IProjectionSystemState State { get; protected set; }
  }

  public class ProjectionSystem<TItem> : ProjectionSystem, IProjectionSystem<TItem>
    where TItem : IProjectedItem {
    readonly ITraceLogger _traceLogger;
    readonly IProjectionSystemState<TItem> _currentState;
    readonly IProjectionSystemState<TItem> _expiredState;
    readonly object _stateLock;
    readonly IProjectionSystemState<TItem> _updatingState;

    public ProjectionSystem(TimeSpan timeout, IProjectionDataService<TItem> projectionDataService, ITraceLogger traceLogger) {
      _traceLogger = traceLogger;
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _currentState = new CurrentState<TItem>(timeout);
      _expiredState = new ExpiredState<TItem>();
      _updatingState = new UpdatingState<TItem>(projectionDataService);
      State = _expiredState; // Expired on start
      _stateLock = new object();
    }

    public new IProjectionSystemState<TItem> State {
      get { return base.State as IProjectionSystemState<TItem>; }
      protected set { base.State = value; }
    }

    public void SwitchToExpiredState() {
      lock (_stateLock) {
        var previousState = State;
        State = _expiredState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _expiredState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void SwitchToUpdatingState() {
      lock (_stateLock) {
        var previousState = State;
        State = _updatingState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _updatingState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void SwitchToCurrentState() {
      lock (_stateLock) {
        var previousState = State;
        State = _currentState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _currentState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }
  }
}