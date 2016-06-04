using System;
using System.Collections.Generic;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class MaintainingState : MaintainingState<Department> {
    public MaintainingState(IEnumerable<Department> oldProjectedData, TimeSpan timeout, IProjectionDataService<Department> departmentsProjectionDataService)
      : base(oldProjectedData, timeout, departmentsProjectionDataService) {}
  }
}