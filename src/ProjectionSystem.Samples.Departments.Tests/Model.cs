using System;
using System.Threading.Tasks;
using DavidLievrouw.Utils;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public static class Model {
    public static IProjectionSystem<Department> Create(TimeSpan expiration, TimeSpan updateDuration) {
      var stateSyncLockFactory = new RealSyncLockFactory(new object());
      var systemClock = new RealSystemClock();
      var traceLogger = new ConsoleTraceLogger(systemClock);
      var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
      var createProjectionLockFactory = new RealSyncLockFactory(new object());
      var updateProjectionLockFactory = new RealSyncLockFactory(new object());
      var transitionGuardFactory = new StateTransitionGuardFactory();
      var projectionDataService = new DepartmentsProjectionDataService(updateDuration, systemClock, traceLogger);

      return new DepartmentsProjectionSystemFactory(stateSyncLockFactory, traceLogger).Create(
        new UninitialisedState<Department>(transitionGuardFactory),
        new CurrentState<Department>(transitionGuardFactory, expiration, taskScheduler),
        new ExpiredState<Department>(transitionGuardFactory),
        new UpdatingState<Department>(transitionGuardFactory, projectionDataService, updateProjectionLockFactory, taskScheduler),
        new CreatingState<Department>(transitionGuardFactory, projectionDataService, createProjectionLockFactory));
    }
  }
}