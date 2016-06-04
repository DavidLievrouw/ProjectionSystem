using System;
using System.Threading.Tasks;

namespace ProjectionSystem {
  public abstract class ProjectionSystem : IProjectionSystem {
    public abstract Task EnterState(IProjectionSystemState state);
    public IProjectionSystemState State { get; protected set; }
  }

  public class ProjectionSystem<TItem> : ProjectionSystem, IProjectionSystem<TItem> where TItem : IProjectedItem {
    public override async Task EnterState(IProjectionSystemState state) {
      if (state == null) throw new ArgumentNullException(nameof(state));
      await state.Enter(this).ConfigureAwait(false);
      base.State = state;
    }

    public new IProjectionSystemState<TItem> State {
      get { return base.State as IProjectionSystemState<TItem>; }
      protected set { base.State = value; }
    }
  }
}