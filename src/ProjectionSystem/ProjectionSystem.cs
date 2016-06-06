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

    public ProjectionSystem(
      IState<TItem> uninitialisedState,
      IState<TItem> creatingState,
      IState<TItem> currentState,
      IState<TItem> expiredState,
      IState<TItem> updatingState,
      ITraceLogger traceLogger) {
      if (uninitialisedState == null) throw new ArgumentNullException(nameof(uninitialisedState));
      if (creatingState == null) throw new ArgumentNullException(nameof(creatingState));
      if (currentState == null) throw new ArgumentNullException(nameof(currentState));
      if (expiredState == null) throw new ArgumentNullException(nameof(expiredState));
      if (updatingState == null) throw new ArgumentNullException(nameof(updatingState));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      _creatingState = creatingState;
      _currentState = currentState;
      _expiredState = expiredState;
      _updatingState = updatingState;
      _traceLogger = traceLogger;

      State = uninitialisedState;
    }

    public IState<TItem> State { get; private set; }

    IState IProjectionSystem.State => State;

    public async Task TransitionToExpiredState() {
      var previousState = State;
      State = _expiredState;
      _traceLogger.Verbose($"Entering '{State.Id}' state.");
      await _expiredState.Enter(this, previousState);
      _traceLogger.Verbose($"Entered '{State.Id}' state.");
    }

    public async Task TransitionToCreatingState() {
      var previousState = State;
      State = _creatingState;
      _traceLogger.Verbose($"Entering '{State.Id}' state.");
      await _creatingState.Enter(this, previousState);
      _traceLogger.Verbose($"Entered '{State.Id}' state.");
    }

    public async Task TransitionToUpdatingState() {
      var previousState = State;
      State = _updatingState;
      _traceLogger.Verbose($"Entering '{State.Id}' state.");
      await _updatingState.Enter(this, previousState);
      _traceLogger.Verbose($"Entered '{State.Id}' state.");
    }

    public async Task TransitionToCurrentState() {
      var previousState = State;
      State = _currentState;
      _traceLogger.Verbose($"Entering '{State.Id}' state.");
      await _currentState.Enter(this, previousState);
      _traceLogger.Verbose($"Entered '{State.Id}' state.");
    }

    public async Task<IEnumerable<TItem>> GetProjection() {
      if (State.Id == StateId.Uninitialised) await TransitionToCreatingState();
      if (State.Id == StateId.Expired) await TransitionToUpdatingState();
      return await State.GetProjection();
    }
  }
}