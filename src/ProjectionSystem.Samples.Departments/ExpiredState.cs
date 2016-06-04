using System.Collections.Generic;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class ExpiredState : ExpiredState<Department> {
    public ExpiredState(IEnumerable<Department> invalidData) : base(invalidData) {}
  }
}