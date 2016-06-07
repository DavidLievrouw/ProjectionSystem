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
      IStateTransitionOrchestrator<TItem> stateTransitionOrchestrator,
      IStateFactory<TItem> uninitialisedStateFactory,
      IStateFactory<TItem> creatingStateFactory,
      IStateFactory<TItem> currentStateFactory,
      IStateFactory<TItem> expiredStateFactory,
      IStateFactory<TItem> updatingStateFactory,
      ISyncLockFactory getProjectionLockFactory) {
      if (stateTransitionOrchestrator == null) throw new ArgumentNullException(nameof(stateTransitionOrchestrator));
      if (uninitialisedStateFactory == null) throw new ArgumentNullException(nameof(uninitialisedStateFactory));
      if (creatingStateFactory == null) throw new ArgumentNullException(nameof(creatingStateFactory));
      if (currentStateFactory == null) throw new ArgumentNullException(nameof(currentStateFactory));
      if (expiredStateFactory == null) throw new ArgumentNullException(nameof(expiredStateFactory));
      if (updatingStateFactory == null) throw new ArgumentNullException(nameof(updatingStateFactory));
      if (getProjectionLockFactory == null) throw new ArgumentNullException(nameof(getProjectionLockFactory));
      _stateTransitionOrchestrator = stateTransitionOrchestrator;
      _creatingState = creatingStateFactory.Create(this);
      _currentState = currentStateFactory.Create(this);
      _expiredState = expiredStateFactory.Create(this);
      _updatingState = updatingStateFactory.Create(this);
      _getProjectionLockFactory = getProjectionLockFactory;

      _stateTransitionOrchestrator.TransitionToState(uninitialisedStateFactory.Create(this));
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