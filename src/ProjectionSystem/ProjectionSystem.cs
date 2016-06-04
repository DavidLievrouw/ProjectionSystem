using System;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public abstract class ProjectionSystem : IProjectionSystem {
    public abstract void EnterState(IProjectionSystemState state);
    public IProjectionSystemState State { get; protected set; }
  }

  public class ProjectionSystem<TItem> : ProjectionSystem, IProjectionSystem<TItem> where TItem : IProjectedItem {
    public override void EnterState(IProjectionSystemState state) {
      if (state == null) throw new ArgumentNullException(nameof(state));
      var previousState = base.State;
      base.State = state;
      state.Enter(this, previousState);
    }

    public new IProjectionSystemState<TItem> State {
      get { return base.State as IProjectionSystemState<TItem>; }
      protected set { base.State = value; }
    }
  }
}