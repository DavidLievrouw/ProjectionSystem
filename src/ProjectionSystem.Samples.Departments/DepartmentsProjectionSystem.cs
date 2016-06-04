using System;
using System.Collections.Generic;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class DepartmentsProjectionSystem : ProjectionSystem<Department> {
    public DepartmentsProjectionSystem(TimeSpan expiration, IProjectionDataService<Department> departmentsProjectionDataService) : base(expiration, departmentsProjectionDataService) {
      State = new ExpiredState(); // Initialise to expired
    }

    public IEnumerable<Department> GetProjectedDepartments() {
      if (State.Id == StateId.Expired) SwitchToUpdatingState();
      return State.GetProjectedData();
    }
  }
}