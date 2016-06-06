using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public class ProjectionSystem<TItem> : IProjectionSystem<TItem>
    where TItem : IProjectedItem {
    readonly IState<TItem> _creatingState;
    readonly IState<TItem> _currentState;
    readonly IState<TItem> _expiredState;
    readonly IState<TItem> _updatingState;
    readonly ITraceLogger _traceLogger;
    readonly ISyncLockFactory _stateTransitionLockFactory;

    public ProjectionSystem(
      IState<TItem> uninitialisedState,
      IState<TItem> creatingState,
      IState<TItem> currentState,
      IState<TItem> expiredState,
      IState<TItem> updatingState,
      ITraceLogger traceLogger,
      ISyncLockFactory stateTransitionLockFactory) {
      if (uninitialisedState == null) throw new ArgumentNullException(nameof(uninitialisedState));
      if (creatingState == null) throw new ArgumentNullException(nameof(creatingState));
      if (currentState == null) throw new ArgumentNullException(nameof(currentState));
      if (expiredState == null) throw new ArgumentNullException(nameof(expiredState));
      if (updatingState == null) throw new ArgumentNullException(nameof(updatingState));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      if (stateTransitionLockFactory == null) throw new ArgumentNullException(nameof(stateTransitionLockFactory));
      _creatingState = creatingState;
      _currentState = currentState;
      _expiredState = expiredState;
      _updatingState = updatingState;
      _traceLogger = traceLogger;
      _stateTransitionLockFactory = stateTransitionLockFactory;

      State = uninitialisedState;
    }

    public IState<TItem> State { get; private set; }

    public async Task TransitionToExpiredState() {
      _traceLogger.Verbose($"Entering '{State.Id}' state.");
      await _expiredState.Prepare(this);
      State = _expiredState;
      await _expiredState.Enter(this);
      _traceLogger.Verbose($"Entered '{State.Id}' state.");
    }

    public async Task TransitionToCreatingState() {
      _traceLogger.Verbose($"Entering '{State.Id}' state.");
      await _creatingState.Prepare(this);
      State = _creatingState;
      await _creatingState.Enter(this);
      _traceLogger.Verbose($"Entered '{State.Id}' state.");
    }

    public async Task TransitionToUpdatingState() {
      _traceLogger.Verbose($"Entering '{State.Id}' state.");
      await _updatingState.Prepare(this);
      State = _updatingState;
      await _updatingState.Enter(this);
      _traceLogger.Verbose($"Entered '{State.Id}' state.");
    }

    public async Task TransitionToCurrentState() {
      _traceLogger.Verbose($"Entering '{State.Id}' state.");
      await _currentState.Prepare(this);
      State = _currentState;
      await _currentState.Enter(this);
      _traceLogger.Verbose($"Entered '{State.Id}' state.");
    }

    public async Task<IEnumerable<TItem>> GetProjection() {
      using (await _stateTransitionLockFactory.Create()) {
        if (State.Id == StateId.Uninitialised) await TransitionToCreatingState();
        if (State.Id == StateId.Expired) await TransitionToUpdatingState();
        return await State.GetProjection();
      }
    }
  }
}