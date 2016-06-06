using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public abstract class State : IState {
    public abstract void Enter(IProjectionSystem projectionSysten, IState previousState);
    public abstract StateId Id { get; }
  }

  public abstract class State<TItem> : State, IState<TItem>
    where TItem : IProjectedItem {
    public abstract void Enter(IProjectionSystem<TItem> projectionSystem, IState<TItem> previousState);
    public abstract IEnumerable<TItem> GetProjection();

    void IState.Enter(IProjectionSystem projectionSystem, IState previousState) {
      Enter(projectionSystem, previousState);
    }

    public override void Enter(IProjectionSystem projectionSystem, IState previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var typedProjectionSystem = projectionSystem as IProjectionSystem<TItem>;
      if (typedProjectionSystem == null) throw new ArgumentException($"The {GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(projectionSystem));
      var typedProjectionSystemState = previousState as IState<TItem>;
      if (typedProjectionSystemState == null) throw new ArgumentException($"The previous state '{previousState.GetType().Name}' cannot be used by a {projectionSystem.GetType().Name}.", nameof(previousState));
      Enter(typedProjectionSystem, typedProjectionSystemState);
    }
  }
}