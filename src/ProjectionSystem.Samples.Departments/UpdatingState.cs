using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class UpdatingState : UpdatingState<Department> {
    public UpdatingState(IProjectionDataService<Department> departmentsProjectionDataService)
      : base(departmentsProjectionDataService) {}
  }
}