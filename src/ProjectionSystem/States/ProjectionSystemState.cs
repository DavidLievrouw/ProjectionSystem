using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public abstract class ProjectionSystemState : IProjectionSystemState {
    public abstract Task Enter(IProjectionSystem projectionSystem, IProjectionSystemState previousState);
    public abstract StateId Id { get; }

    protected void StateTransitionGuard(IEnumerable<StateId> allowedStates, StateId originalStateId) {
      var invalidTransitionException = new InvalidOperationException($"State '{originalStateId}' cannot handle a change to state '{Id}'.");
      if (allowedStates == null || !allowedStates.Contains(originalStateId)) throw invalidTransitionException;
    }
  }

  public abstract class ProjectionSystemState<TItem> : ProjectionSystemState, IProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    public abstract Task Enter(IProjectionSystem<TItem> projectionSystemm, IProjectionSystemState<TItem> previousState);
    public abstract Task<IEnumerable<TItem>> GetProjectedData();

    Task IProjectionSystemState.Enter(IProjectionSystem projectionSystem, IProjectionSystemState previousState) {
      return Enter(projectionSystem, previousState);
    }

    public override Task Enter(IProjectionSystem projectionSystem, IProjectionSystemState previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var typedProjectionSystem = projectionSystem as IProjectionSystem<TItem>;
      if (typedProjectionSystem == null) throw new ArgumentException($"The {GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(projectionSystem));
      var typedProjectionSystemState = previousState as IProjectionSystemState<TItem>;
      if (typedProjectionSystemState == null) throw new ArgumentException($"The previous state of {previousState.GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(previousState));
      return Enter(typedProjectionSystem, typedProjectionSystemState);
    }
  }
}