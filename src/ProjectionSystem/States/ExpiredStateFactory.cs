using System;

namespace ProjectionSystem.States {
  public class ExpiredStateFactory<TItem> : IStateFactory<TItem>
    where TItem : IProjectedItem {

    public IState<TItem> Create(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      return new ExpiredState<TItem>(projectionSystem);
    }
  }
}