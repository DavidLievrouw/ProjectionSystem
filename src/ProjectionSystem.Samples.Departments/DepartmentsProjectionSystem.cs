using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class DepartmentsProjectionSystem : ProjectionSystem<Department> {
    readonly ISyncLockFactory _stateSyncLockFactory;

    public DepartmentsProjectionSystem(
      TimeSpan expiration,
      IProjectionDataService<Department> departmentsProjectionDataService,
      ITraceLogger traceLogger,
      ISyncLockFactory stateSyncLockFactory,
      ISyncLockFactory createProjectionLockFactory,
      ISyncLockFactory updateProjectionLockFactory,
      IStateTransitionGuardFactory transitionGuardFactory,
      TaskScheduler taskScheduler)
      : base(expiration, departmentsProjectionDataService, traceLogger, stateSyncLockFactory, createProjectionLockFactory, updateProjectionLockFactory, transitionGuardFactory, taskScheduler) {
      if (stateSyncLockFactory == null) throw new ArgumentNullException(nameof(stateSyncLockFactory));
      _stateSyncLockFactory = stateSyncLockFactory;
    }

    public IEnumerable<Department> GetProjectedDepartments() {
      using (_stateSyncLockFactory.Create()) {
        if (State.Id == StateId.Uninitialised) TransitionToCreatingState();
        if (State.Id == StateId.Expired) TransitionToUpdatingState();
      }
      return State.GetProjectedData();
    }
  }
}