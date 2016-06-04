using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem.Samples.Departments {
  public class MaintainingState : ProjectionSystemState<Department> {
    IEnumerable<Department> _projectedData;
    readonly TimeSpan _timeout;
    readonly IProjectionDataService<Department> _departmentsProjectionDataService;
    bool _isMaintaining;
    readonly SemaphoreSlim _semaphore;

    public MaintainingState(IEnumerable<Department> oldProjectedData, TimeSpan timeout, IProjectionDataService<Department> departmentsProjectionDataService) {
      if (departmentsProjectionDataService == null) throw new ArgumentNullException(nameof(departmentsProjectionDataService));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _projectedData = oldProjectedData; // Can be null the first time
      _timeout = timeout;
      _departmentsProjectionDataService = departmentsProjectionDataService;
      _semaphore = new SemaphoreSlim(1);
      _isMaintaining = false;
    }

    public override async Task Enter(IProjectionSystem<Department> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      StateTransitionGuard(
        allowedStates: new[] { ProjectionState.Expired },
        originalState: projectionSystem.State.Id);

      try {
        _isMaintaining = true;

        // Make sure only one refresh action is done at a time
        await _semaphore.WaitAsync().ConfigureAwait(false);
        try {
          await _departmentsProjectionDataService.RefreshProjection().ConfigureAwait(false);
          _projectedData = await _departmentsProjectionDataService.GetProjection().ConfigureAwait(false);
        }
        finally {
          _semaphore.Release();
        }
      }
      finally {
        _isMaintaining = false;
      }

      await projectionSystem.EnterState(new CurrentState(_projectedData, _timeout)).ConfigureAwait(false);
    }

    public override ProjectionState Id => ProjectionState.Maintaining;

    public override async Task<IEnumerable<Department>> GetProjectedData() {
      // If currently busy and old data is present? Shortcut out of here.
      var oldData = _projectedData;
      if (_isMaintaining && oldData != null) return oldData;

      // Block until refresh is finished (only when no old data is available)
      await _semaphore.WaitAsync().ConfigureAwait(false);
      try {
        return _projectedData;
      }
      finally {
        _semaphore.Release();
      }
    }
  }
}