using System;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class CurrentState : CurrentState<Department> {
    public CurrentState(TimeSpan timeout) : base(timeout) {}
  }
}