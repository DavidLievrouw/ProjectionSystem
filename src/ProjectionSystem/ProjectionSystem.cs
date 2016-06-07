using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectionSystem.States;
using ProjectionSystem.States.Transitions;

namespace ProjectionSystem {
  public class ProjectionSystem<TItem> : IProjectionSystem<TItem>
    where TItem : IProjectedItem {
    readonly IStateTransitionOrchestrator<TItem> _stateTransitionOrchestrator;
    readonly IState<TItem> _creatingState;
    readonly IState<TItem> _currentState;
    readonly IState<TItem> _expiredState;
    readonly IState<TItem> _updatingState;
    readonly ISyncLockFactory _getProjectionLockFactory;

    public ProjectionSystem(
      IStateTransitionOrchestratorFactory<TItem> stateTransitionOrchestratorFactory,
      IState<TItem> uninitialisedState,
      IState<TItem> creatingState,
      IState<TItem> currentState,
      IState<TItem> expiredState,
      IState<TItem> updatingState,
      ISyncLockFactory getProjectionLockFactory) {
      if (stateTransitionOrchestratorFactory == null) throw new ArgumentNullException(nameof(stateTransitionOrchestratorFactory));
      if (uninitialisedState == null) throw new ArgumentNullException(nameof(uninitialisedState));
      if (creatingState == null) throw new ArgumentNullException(nameof(creatingState));
      if (currentState == null) throw new ArgumentNullException(nameof(currentState));
      if (expiredState == null) throw new ArgumentNullException(nameof(expiredState));
      if (updatingState == null) throw new ArgumentNullException(nameof(updatingState));
      if (getProjectionLockFactory == null) throw new ArgumentNullException(nameof(getProjectionLockFactory));
      _creatingState = creatingState;
      _currentState = currentState;
      _expiredState = expiredState;
      _updatingState = updatingState;
      _getProjectionLockFactory = getProjectionLockFactory;

      _stateTransitionOrchestrator = stateTransitionOrchestratorFactory.CreateFor(this);
      _stateTransitionOrchestrator.TransitionToState(uninitialisedState);
    }

    public async Task InvalidateProjection() {
      await _stateTransitionOrchestrator.TransitionToState(_expiredState);
    }

    public async Task MarkProjectionAsUpToDate() {
      await _stateTransitionOrchestrator.TransitionToState(_currentState);
    }

    public async Task<IEnumerable<TItem>> GetProjection() {
      using (await _getProjectionLockFactory.Create()) {
        if (State.Id == StateId.Uninitialised) await TransitionToCreatingState();
        if (State.Id == StateId.Expired) await TransitionToUpdatingState();
        return await State.GetProjection();
      }
    }

    public IState<TItem> State => _stateTransitionOrchestrator.CurrentState;

    async Task TransitionToCreatingState() {
      await _stateTransitionOrchestrator.TransitionToState(_creatingState);
    }

    async Task TransitionToUpdatingState() {
      await _stateTransitionOrchestrator.TransitionToState(_updatingState);
    }
  }
}