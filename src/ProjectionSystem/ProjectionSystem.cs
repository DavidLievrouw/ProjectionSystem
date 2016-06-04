using System;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public abstract class ProjectionSystem : IProjectionSystem {
    public IProjectionSystemState State { get; protected set; }
  }

  public class ProjectionSystem<TItem> : ProjectionSystem, IProjectionSystem<TItem> where TItem : IProjectedItem {
    readonly IProjectionSystemState<TItem> _currentState;
    readonly IProjectionSystemState<TItem> _updatingState;
    readonly IProjectionSystemState<TItem> _expiredState;

    public ProjectionSystem(TimeSpan timeout, IProjectionDataService<TItem> projectionDataService) {
      if (projectionDataService == null) throw new ArgumentNullException(nameof(projectionDataService));
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _currentState = new CurrentState<TItem>(timeout);
      _expiredState = new ExpiredState<TItem>();
      _updatingState = new UpdatingState<TItem>(projectionDataService);
      State = _expiredState; // Expired on start
    }

    public new IProjectionSystemState<TItem> State {
      get { return base.State as IProjectionSystemState<TItem>; }
      protected set { base.State = value; }
    }

    public void SwitchToExpiredState() {
      _expiredState.Enter(this, State);
      State = _expiredState;
    }

    public void SwitchToUpdatingState() {
      _updatingState.Enter(this, State);
      State = _updatingState;
    }

    public void SwitchToCurrentState() {
      _currentState.Enter(this, State);
      State = _currentState;
    }
  }
}