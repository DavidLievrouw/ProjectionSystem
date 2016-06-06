using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class DepartmentsProjectionSystem : ProjectionSystem<Department> {
    readonly ISyncLockFactory _syncLockFactory;
    readonly object _updateStateLockObj;

    public DepartmentsProjectionSystem(
      TimeSpan expiration,
      IProjectionDataService<Department> departmentsProjectionDataService,
      ITraceLogger traceLogger,
      ISyncLockFactory syncLockFactory,
      TaskScheduler taskScheduler) : base(expiration, departmentsProjectionDataService, traceLogger, syncLockFactory, taskScheduler) {
      if (syncLockFactory == null) throw new ArgumentNullException(nameof(syncLockFactory));
      _syncLockFactory = syncLockFactory;
      _updateStateLockObj = new object();
    }

    public IEnumerable<Department> GetProjectedDepartments() {
      using (_syncLockFactory.CreateFor(_updateStateLockObj)) {
        if (State.Id == StateId.Uninitialised) SwitchToCreatingState();
        if (State.Id == StateId.Expired) SwitchToUpdatingState();
      }
      return State.GetProjectedData();
    }
  }
}