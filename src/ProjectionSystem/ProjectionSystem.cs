using System;
using System.Collections.Generic;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public class ProjectionSystem<TItem> : IProjectionSystem<TItem>
    where TItem : IProjectedItem {
    readonly IState<TItem> _creatingState;
    readonly IState<TItem> _currentState;
    readonly IState<TItem> _expiredState;
    readonly IState<TItem> _updatingState;
    readonly ISyncLockFactory _stateLockFactory;
    readonly ITraceLogger _traceLogger;

    public ProjectionSystem(
      IState<TItem> uninitialisedState,
      IState<TItem> creatingState,
      IState<TItem> currentState,
      IState<TItem> expiredState,
      IState<TItem> updatingState,
      ISyncLockFactory stateLockFactory,
      ITraceLogger traceLogger) {
      if (uninitialisedState == null) throw new ArgumentNullException(nameof(uninitialisedState));
      if (creatingState == null) throw new ArgumentNullException(nameof(creatingState));
      if (currentState == null) throw new ArgumentNullException(nameof(currentState));
      if (expiredState == null) throw new ArgumentNullException(nameof(expiredState));
      if (updatingState == null) throw new ArgumentNullException(nameof(updatingState));
      if (stateLockFactory == null) throw new ArgumentNullException(nameof(stateLockFactory));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      _creatingState = creatingState;
      _currentState = currentState;
      _expiredState = expiredState;
      _updatingState = updatingState;
      _stateLockFactory = stateLockFactory;
      _traceLogger = traceLogger;

      State = uninitialisedState;
    }

    public IState<TItem> State { get; private set; }

    IState IProjectionSystem.State => State;

    public void TransitionToExpiredState() {
      using (_stateLockFactory.Create()) {
        var previousState = State;
        State = _expiredState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _expiredState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void TransitionToCreatingState() {
      using (_stateLockFactory.Create()) {
        var previousState = State;
        State = _creatingState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _creatingState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void TransitionToUpdatingState() {
      using (_stateLockFactory.Create()) {
        var previousState = State;
        State = _updatingState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _updatingState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public void TransitionToCurrentState() {
      using (_stateLockFactory.Create()) {
        var previousState = State;
        State = _currentState;
        _traceLogger.Verbose($"Entering '{State.Id}' state.");
        _currentState.Enter(this, previousState);
        _traceLogger.Verbose($"Entered '{State.Id}' state.");
      }
    }

    public IEnumerable<TItem> GetProjection() {
      using (_stateLockFactory.Create()) {
        if (State.Id == StateId.Uninitialised) TransitionToCreatingState();
        if (State.Id == StateId.Expired) TransitionToUpdatingState();
      }
      return State.GetProjection();
    }
  }
}