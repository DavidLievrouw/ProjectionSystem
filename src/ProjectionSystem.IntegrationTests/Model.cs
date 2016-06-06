using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DavidLievrouw.Utils;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.IntegrationTests.Items;
using ProjectionSystem.States;
using ProjectionSystem.States.Transitions;

namespace ProjectionSystem.IntegrationTests {
  public static class Model {
    public static IProjectionSystem<Department> Create(TimeSpan expiration, IProjectionDataService<Department> projectionDataService) {
      var systemClock = new RealSystemClock();
      var traceLogger = new ConsoleTraceLogger(systemClock);
      var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
      var createProjectionLockFactory = new RealSyncLockFactory(new SemaphoreSlim(1));
      var updateProjectionLockFactory = new RealSyncLockFactory(new SemaphoreSlim(1));
      var stateTransitionLockFactory = new RealSyncLockFactory(new SemaphoreSlim(1));
      var transitionGuardFactory = new StateTransitionGuardFactory();

      return new ProjectionSystem<Department>(
        new UninitialisedState<Department>(transitionGuardFactory),
        new CreatingState<Department>(transitionGuardFactory, projectionDataService, createProjectionLockFactory),
        new CurrentState<Department>(transitionGuardFactory, expiration, taskScheduler),
        new ExpiredState<Department>(transitionGuardFactory),
        new UpdatingState<Department>(transitionGuardFactory, projectionDataService, updateProjectionLockFactory, taskScheduler),
        traceLogger,
        stateTransitionLockFactory);
    }

    public class ProjectionDataServiceForTest : IProjectionDataService<Department> {
      readonly TimeSpan _updateDuration;
      readonly ISystemClock _systemClock;
      readonly ITraceLogger _traceLogger;
      readonly IEnumerable<Department> _projection;
      int _refreshCount;

      public ProjectionDataServiceForTest(TimeSpan updateDuration) {
        if (updateDuration <= TimeSpan.Zero) throw new ArgumentException("An invalid refresh duration has been specified.", nameof(updateDuration));
        _updateDuration = updateDuration;
        _systemClock = new RealSystemClock();
        _traceLogger = new ConsoleTraceLogger(_systemClock);
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

      public int RefreshCount => _refreshCount;

      public Task<IEnumerable<Department>> GetProjection() {
        return Task.FromResult(_projection);
      }

      public async Task UpdateProjection() {
        // Fake update the projection
        _traceLogger.Verbose("Refreshing projection...");
        await Task.Delay(_updateDuration);
        foreach (var department in _projection) {
          department.ProjectionTime = _systemClock.UtcNow;
        }
        _refreshCount++;
        _traceLogger.Verbose($"Refreshed projection for the {RefreshCount}th time.");
      }
    }
  }
}