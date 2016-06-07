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
    readonly IState<TItem> _validState;
    readonly IState<TItem> _expiredState;
    readonly IState<TItem> _updatingState;
    readonly ISyncLockFactory _getProjectionLockFactory;

    public ProjectionSystem(
      IStateTransitionOrchestrator<TItem> stateTransitionOrchestrator,
      IStateFactory<TItem> uninitialisedStateFactory,
      IStateFactory<TItem> creatingStateFactory,
      IStateFactory<TItem> validStateFactory,
      IStateFactory<TItem> expiredStateFactory,
      IStateFactory<TItem> updatingStateFactory,
      ISyncLockFactory getProjectionLockFactory) {
      if (stateTransitionOrchestrator == null) throw new ArgumentNullException(nameof(stateTransitionOrchestrator));
      if (uninitialisedStateFactory == null) throw new ArgumentNullException(nameof(uninitialisedStateFactory));
      if (creatingStateFactory == null) throw new ArgumentNullException(nameof(creatingStateFactory));
      if (validStateFactory == null) throw new ArgumentNullException(nameof(validStateFactory));
      if (expiredStateFactory == null) throw new ArgumentNullException(nameof(expiredStateFactory));
      if (updatingStateFactory == null) throw new ArgumentNullException(nameof(updatingStateFactory));
      if (getProjectionLockFactory == null) throw new ArgumentNullException(nameof(getProjectionLockFactory));
      _stateTransitionOrchestrator = stateTransitionOrchestrator;
      _creatingState = creatingStateFactory.Create(this);
      _validState = validStateFactory.Create(this);
      _expiredState = expiredStateFactory.Create(this);
      _updatingState = updatingStateFactory.Create(this);
      _getProjectionLockFactory = getProjectionLockFactory;

      var initialisationTask = _stateTransitionOrchestrator
        .TransitionToState(uninitialisedStateFactory.Create(this));
      if (!initialisationTask.IsCompleted) initialisationTask.RunSynchronously();
    }

    public async Task InvalidateProjection() {
      await _stateTransitionOrchestrator.TransitionToState(_expiredState);
    }

    public async Task MarkProjectionAsUpToDate() {
      await _stateTransitionOrchestrator.TransitionToState(_validState);
    }

    public async Task<IEnumerable<TItem>> GetProjection() {
      using (await _getProjectionLockFactory.Create()) {
        if (State.Id == StateId.Uninitialised) await TransitionToCreatingState();
        if (State.Id == StateId.Expired) await TransitionToUpdatingState();
        return await State.GetProjection();
      }
    }

    public IState<TItem> State => _stateTransitionOrchestrator.CurrentState;

    Task TransitionToCreatingState() {
      return _stateTransitionOrchestrator.TransitionToState(_creatingState);
    }

    Task TransitionToUpdatingState() {
      return _stateTransitionOrchestrator.TransitionToState(_updatingState);
    }
  }
}