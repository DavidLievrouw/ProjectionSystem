using System;
using System.Threading.Tasks;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public abstract class ProjectionSystem : IProjectionSystem {
    public abstract Task EnterState(IProjectionSystemState state);
    public IProjectionSystemState State { get; protected set; }
  }

  public class ProjectionSystem<TItem> : ProjectionSystem, IProjectionSystem<TItem> where TItem : IProjectedItem {
    public override async Task EnterState(IProjectionSystemState state) {
      if (state == null) throw new ArgumentNullException(nameof(state));
      var previousState = base.State;
      base.State = state;
      await state.Enter(this, previousState).ConfigureAwait(false);
    }

    public new IProjectionSystemState<TItem> State {
      get { return base.State as IProjectionSystemState<TItem>; }
      protected set { base.State = value; }
    }
  }
}