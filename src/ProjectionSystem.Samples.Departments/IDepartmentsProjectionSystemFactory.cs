using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public interface IDepartmentsProjectionSystemFactory {
    IProjectionSystem<Department> Create(
      IState<Department> uninitialisedState,
      IState<Department> currentState,
      IState<Department> expiredState,
      IState<Department> updatingState,
      IState<Department> creatingState);
  }
}