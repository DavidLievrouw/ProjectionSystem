using System;
using System.Collections.Generic;
using System.Threading;
using DavidLievrouw.Utils;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.Samples.Departments.Items;

namespace ProjectionSystem.Samples.Departments {
  public class DepartmentsProjectionDataService : IProjectionDataService<Department> {
    readonly ISystemClock _systemClock;
    readonly ITraceLogger _traceLogger;
    readonly IEnumerable<Department> _projection;
    int _refreshCount;

    public DepartmentsProjectionDataService(ISystemClock systemClock, ITraceLogger traceLogger) {
      if (systemClock == null) throw new ArgumentNullException(nameof(systemClock));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      _systemClock = systemClock;
      _traceLogger = traceLogger;
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
      _traceLogger.Verbose("Refreshing projection...");
      //var delayMillis = new Random().Next(500, 3000);
      var delayMillis = 500;
      Thread.Sleep(delayMillis);
      foreach (var department in _projection) {
        department.ProjectionTime = _systemClock.UtcNow;
      }
      _refreshCount++;
      _traceLogger.Verbose($"Refreshed projection for the {RefreshCount}th time.");
    }

    public int RefreshCount => _refreshCount;
  }
}