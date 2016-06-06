using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectionSystem.States {
  public abstract class ProjectionSystemState : IProjectionSystemState {
    public abstract void Enter(IProjectionSystem projectionSystem, IProjectionSystemState previousState);
    public abstract StateId Id { get; }

    protected void StateTransitionGuard(IEnumerable<StateId> allowedStates, StateId? originalStateId) {
      var invalidTransitionException = new InvalidOperationException($"State '{originalStateId}' cannot handle a change to state '{Id}'.");
      if (!originalStateId.HasValue) return;
      if (allowedStates == null || !allowedStates.Contains(originalStateId.Value)) throw invalidTransitionException;
    }
  }

  public abstract class ProjectionSystemState<TItem> : ProjectionSystemState, IProjectionSystemState<TItem>
    where TItem : IProjectedItem {
    public abstract void Enter(IProjectionSystem<TItem> projectionSystemm, IProjectionSystemState<TItem> previousState);
    public abstract IEnumerable<TItem> GetProjectedData();

    void IProjectionSystemState.Enter(IProjectionSystem projectionSystem, IProjectionSystemState previousState) {
      Enter(projectionSystem, previousState);
    }

    public override void Enter(IProjectionSystem projectionSystem, IProjectionSystemState previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var typedProjectionSystem = projectionSystem as IProjectionSystem<TItem>;
      if (typedProjectionSystem == null) throw new ArgumentException($"The {GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(projectionSystem));
      var typedProjectionSystemState = previousState as IProjectionSystemState<TItem>;
      if (typedProjectionSystemState == null) throw new ArgumentException($"The previous state of {previousState.GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(previousState));
      Enter(typedProjectionSystem, typedProjectionSystemState);
    }
  }
}