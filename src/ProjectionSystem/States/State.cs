using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public abstract class State : IState {
    public abstract Task Enter(IProjectionSystem projectionSysten, IState previousState);
    public abstract StateId Id { get; }
  }

  public abstract class State<TItem> : State, IState<TItem>
    where TItem : IProjectedItem {
    public abstract Task Enter(IProjectionSystem<TItem> projectionSystem, IState<TItem> previousState);
    public abstract Task<IEnumerable<TItem>> GetProjection();

    Task IState.Enter(IProjectionSystem projectionSystem, IState previousState) {
      return Enter(projectionSystem, previousState);
    }

    public override Task Enter(IProjectionSystem projectionSystem, IState previousState) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var typedProjectionSystem = projectionSystem as IProjectionSystem<TItem>;
      if (typedProjectionSystem == null) throw new ArgumentException($"The {GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(projectionSystem));
      var typedProjectionSystemState = previousState as IState<TItem>;
      if (typedProjectionSystemState == null) throw new ArgumentException($"The previous state '{previousState.GetType().Name}' cannot be used by a {projectionSystem.GetType().Name}.", nameof(previousState));
      return Enter(typedProjectionSystem, typedProjectionSystemState);
    }
  }
}