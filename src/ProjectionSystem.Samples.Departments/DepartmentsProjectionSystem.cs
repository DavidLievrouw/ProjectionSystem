using System;
using System.Collections.Generic;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class DepartmentsProjectionSystem : ProjectionSystem<Department> {
    readonly object _syncRoot;

    public DepartmentsProjectionSystem(TimeSpan expiration, IProjectionDataService<Department> departmentsProjectionDataService) : base(expiration, departmentsProjectionDataService) {
      State = new ExpiredState(); // Initialise to expired
      _syncRoot = new object();
    }

    public IEnumerable<Department> GetProjectedDepartments() {
      lock (_syncRoot) {
        if (State.Id == StateId.Expired) SwitchToUpdatingState();
      }
      return State.GetProjectedData();
    }
  }
}