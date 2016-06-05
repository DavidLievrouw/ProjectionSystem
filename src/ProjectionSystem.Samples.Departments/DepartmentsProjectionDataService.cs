using System;
using System.Collections.Generic;
using System.Threading;
using DavidLievrouw.Utils;
using ProjectionSystem.Samples.Departments.Items;

namespace ProjectionSystem.Samples.Departments {
  public class DepartmentsProjectionDataService : IProjectionDataService<Department> {
    readonly ISystemClock _systemClock;
    readonly IEnumerable<Department> _projection;

    public DepartmentsProjectionDataService(ISystemClock systemClock) {
      if (systemClock == null) throw new ArgumentNullException(nameof(systemClock));
      _systemClock = systemClock;
      _projection = new[] {
        new Department {
          Id = 1,
          UniqueIdentifier = Guid.NewGuid(),
          Name = "Cardiology",
          Abbreviation = "cardio",
          ContactName = "Sandy Jefferson",
          PhoneNumber = "005677",
          PhoneType = PhoneType.Phone,
          ProjectionTime = _systemClock.UtcNow
        },
        new Department {
          Id = 2,
          UniqueIdentifier = Guid.NewGuid(),
          Name = "Geriatry",
          Abbreviation = "geria",
          ContactName = "Andrew Dickson",
          PhoneNumber = "005688",
          PhoneType = PhoneType.Phone,
          ProjectionTime = _systemClock.UtcNow
        },
        new Department {
          Id = 3,
          UniqueIdentifier = Guid.NewGuid(),
          Name = "Operating Theatre Unit",
          Abbreviation = "OTU",
          ContactName = "Chris Baker",
          PhoneNumber = "0558-554-52-22",
          PhoneType = PhoneType.Mobile,
          ProjectionTime = _systemClock.UtcNow
        }
      };
    }

    public IEnumerable<Department> GetProjection() {
      return _projection;
    }

    public void RefreshProjection() {
      // Fake update the projection
      //var delayMillis = new Random().Next(500, 3000);
      var delayMillis = 2000;
      Thread.Sleep(delayMillis);
      foreach (var department in _projection) {
        department.ProjectionTime = _systemClock.UtcNow;
      }
      Console.WriteLine("Refreshed projection");
    }
  }
}