using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public class MaintainingState<TItem> : ProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    readonly IProjectionDataService<TItem> _projectionDataService;
    readonly object _syncRoot;
    readonly TimeSpan _timeout;
    IEnumerable<TItem> _projectedData;

    public MaintainingState(IEnumerable<TItem> oldProjectedData, TimeSpan timeout, IProjectionDataService<TItem> projectionDataService) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _projectedData = oldProjectedData; // Can be null the first time
      _timeout = timeout;
      _projectionDataService = projectionDataService;
      _syncRoot = new object();
    }

    public override StateId Id => StateId.Maintaining;

    public override void Enter(IProjectionSystem<TItem> projectionSystem, IProjectionSystemState<TItem> previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        new[] { StateId.Expired },
        projectionSystem.State.Id);

      Task.Run(() => {        
        // Make sure only one refresh action is done at a time
        lock (_syncRoot) {
          _projectionDataService.RefreshProjection();
          _projectedData = _projectionDataService.GetProjection();
        }

        projectionSystem.EnterState(new CurrentState<TItem>(_projectedData, _timeout));
      });
    }

    public override IEnumerable<TItem> GetProjectedData() {
      // Block until refresh is finished
      lock (_syncRoot) {
        return _projectedData;
      }
    }
  }
}