using System;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  public class DepartmentsProjectionSystemFactory : IDepartmentsProjectionSystemFactory {
    readonly ISyncLockFactory _stateLockFactory;
    readonly ITraceLogger _traceLogger;

    public DepartmentsProjectionSystemFactory(
      ISyncLockFactory stateLockFactory,
      ITraceLogger traceLogger) {
      if (stateLockFactory == null) throw new ArgumentNullException(nameof(stateLockFactory));
      if (traceLogger == null) throw new ArgumentNullException(nameof(traceLogger));
      _stateLockFactory = stateLockFactory;
      _traceLogger = traceLogger;
    }

    public IProjectionSystem<Department> Create(
      IState<Department> uninitialisedState,
      IState<Department> currentState,
      IState<Department> expiredState,
      IState<Department> updatingState,
      IState<Department> creatingState) {
      if (uninitialisedState == null) throw new ArgumentNullException(nameof(uninitialisedState));
      if (currentState == null) throw new ArgumentNullException(nameof(currentState));
      if (expiredState == null) throw new ArgumentNullException(nameof(expiredState));
      if (updatingState == null) throw new ArgumentNullException(nameof(updatingState));
      if (creatingState == null) throw new ArgumentNullException(nameof(creatingState));
      return new ProjectionSystem<Department>(
        uninitialisedState,
        creatingState,
        currentState,
        expiredState,
        updatingState,
        _stateLockFactory,
        _traceLogger);
    }
  }
}