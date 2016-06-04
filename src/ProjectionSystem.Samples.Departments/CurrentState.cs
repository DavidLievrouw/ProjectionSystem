using System;
using System.Collections.Generic;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class CurrentState : CurrentState<Department> {
    public CurrentState(IEnumerable<Department> projectedDepartments, TimeSpan timeout) : base(projectedDepartments, timeout) {}
  }
}