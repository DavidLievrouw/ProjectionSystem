using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectionSystem {
  public abstract class ProjectionSystemState : IProjectionSystemState {
    public abstract Task Enter(IProjectionSystem projectionSystem);
    public abstract ProjectionState Id { get; }

    protected void StateTransitionGuard(IEnumerable<ProjectionState> allowedStates, ProjectionState originalState) {
      var invalidTransitionException = new InvalidOperationException($"State '{originalState}' cannot handle a change to state '{Id}'.");
      if (allowedStates == null || !allowedStates.Contains(originalState)) throw invalidTransitionException;
    }
  }

  public abstract class ProjectionSystemState<TItem> : ProjectionSystemState, IProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    public abstract Task Enter(IProjectionSystem<TItem> projectionSystem);
    public abstract Task<IEnumerable<TItem>> GetProjectedData();

    Task IProjectionSystemState.Enter(IProjectionSystem projectionSystem) {
      return Enter(projectionSystem);
    }

    public override Task Enter(IProjectionSystem projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var typedProjectionSystem = projectionSystem as IProjectionSystem<TItem>;
      if (typedProjectionSystem == null) throw new ArgumentException($"The {GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(projectionSystem));
      return Enter(typedProjectionSystem);
    }
  }
}