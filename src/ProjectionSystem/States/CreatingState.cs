using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectionSystem.States.Transitions;

namespace ProjectionSystem.States {
  public class CreatingState<TItem> : State<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly ISyncLockFactory _syncLockFactory;
    readonly IStateTransitionGuardFactory _stateTransitionGuardFactory;
    IEnumerable<TItem> _projectedData;

    public CreatingState(
      IStateTransitionGuardFactory stateTransitionGuardFactory,
      IProjectionDataService<TItem> projectionDataService,
      ISyncLockFactory syncLockFactory) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      if (stateTransitionGuardFactory == null) throw new ArgumentNullException(nameof(stateTransitionGuardFactory));
      _projectionDataService = projectionDataService;
      _syncLockFactory = syncLockFactory;
      _stateTransitionGuardFactory = stateTransitionGuardFactory;
    }

    public override StateId Id => StateId.Creating;

    public override async Task Enter(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var transitionGuard = _stateTransitionGuardFactory.CreateFor(this, new[] { StateId.Uninitialised });
      transitionGuard.PreviousStateRequired(projectionSystem.State);
      transitionGuard.StateTransitionAllowed(projectionSystem.State);

      // Make sure only one refresh action is done at a time
      using (await _syncLockFactory.Create()) {
        await _projectionDataService.UpdateProjection();
        _projectedData = await _projectionDataService.GetProjection();
      }

      projectionSystem.State = this;

      await projectionSystem.TransitionToCurrentState();
    }

    public override async Task<IEnumerable<TItem>> GetProjection() {
      // Do not allow querying of the data, until creation is finished
      using (await _syncLockFactory.Create()) {
        return _projectedData;
      }
    }
  }
}