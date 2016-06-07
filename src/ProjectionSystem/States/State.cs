using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public abstract class State : IState {
    public abstract bool IsTransitionAllowed(StateId? previousState);
    public abstract Task Prepare(IProjectionSystem projectionSystem);
    public abstract Task Enter(IProjectionSystem projectionSystem);
    public abstract StateId Id { get; }
  }

  public abstract class State<TItem> : State, IState<TItem>
    where TItem : IProjectedItem {
    public abstract Task Prepare(IProjectionSystem<TItem> projectionSystem);
    public abstract Task Enter(IProjectionSystem<TItem> projectionSystem);
    public abstract Task<IEnumerable<TItem>> GetProjection();

    Task IState.Prepare(IProjectionSystem projectionSystem) {
      return Prepare(projectionSystem);
    }

    Task IState.Enter(IProjectionSystem projectionSystem) {
      return Enter(projectionSystem);
    }

    public override Task Prepare(IProjectionSystem projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var typedProjectionSystem = projectionSystem as IProjectionSystem<TItem>;
      if (typedProjectionSystem == null) throw new ArgumentException($"The {GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(projectionSystem));
      return Prepare(typedProjectionSystem);
    }

    public override Task Enter(IProjectionSystem projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var typedProjectionSystem = projectionSystem as IProjectionSystem<TItem>;
      if (typedProjectionSystem == null) throw new ArgumentException($"The {GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(projectionSystem));
      return Enter(typedProjectionSystem);
    }
  }
}