using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem.States {
  public abstract class State : IState {
    public abstract bool IsTransitionAllowed(StateId? previousState);
    public abstract Task BeforeEnter(IProjectionSystem projectionSystem);
    public abstract Task AfterEnter(IProjectionSystem projectionSystem);
    public abstract StateId Id { get; }
  }

  public abstract class State<TItem> : State, IState<TItem>
    where TItem : IProjectedItem {
    public abstract Task BeforeEnter(IProjectionSystem<TItem> projectionSystem);
    public abstract Task AfterEnter(IProjectionSystem<TItem> projectionSystem);
    public abstract Task<IEnumerable<TItem>> GetProjection();

    Task IState.BeforeEnter(IProjectionSystem projectionSystem) {
      return BeforeEnter(projectionSystem);
    }

    Task IState.AfterEnter(IProjectionSystem projectionSystem) {
      return AfterEnter(projectionSystem);
    }

    public override Task BeforeEnter(IProjectionSystem projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var typedProjectionSystem = projectionSystem as IProjectionSystem<TItem>;
      if (typedProjectionSystem == null) throw new ArgumentException($"The {GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(projectionSystem));
      return BeforeEnter(typedProjectionSystem);
    }

    public override Task AfterEnter(IProjectionSystem projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      var typedProjectionSystem = projectionSystem as IProjectionSystem<TItem>;
      if (typedProjectionSystem == null) throw new ArgumentException($"The {GetType().Name} cannot be used in a {projectionSystem.GetType().Name}.", nameof(projectionSystem));
      return AfterEnter(typedProjectionSystem);
    }
  }
}