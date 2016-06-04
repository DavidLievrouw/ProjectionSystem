using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class DepartmentsProjectionSystem : ProjectionSystem<Department> {
    readonly IProjectionDataService<Department> _departmentsProjectionDataService;
    readonly TimeSpan _expiration;

    public DepartmentsProjectionSystem(IProjectionDataService<Department> departmentsProjectionDataService, TimeSpan expiration) {
      if (departmentsProjectionDataService == null) throw new ArgumentNullException(nameof(departmentsProjectionDataService));
      if (expiration <= TimeSpan.Zero) throw new ArgumentException("An invalid projection expiration has been specified.", nameof(expiration));
      _departmentsProjectionDataService = departmentsProjectionDataService;
      _expiration = expiration;
      State = new ExpiredState(null); // Initialise to expired
    }

    public async Task<IEnumerable<Department>> GetProjectedDepartments() {
      if (State.Id == StateId.Expired) {
        var currentData = await State.GetProjectedData();
        await EnterState(new MaintainingState(currentData, _expiration, _departmentsProjectionDataService));
      }

      return await State.GetProjectedData();
    }
  }
}