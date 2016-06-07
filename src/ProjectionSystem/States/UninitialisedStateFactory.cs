using System;

namespace ProjectionSystem.States {
  public class UninitialisedStateFactory<TItem> : IStateFactory<TItem>
    where TItem : IProjectedItem {

    public IState<TItem> Create(IProjectionSystem<TItem> projectionSystem) {
      if (projectionSystem == null) throw new ArgumentNullException(nameof(projectionSystem));
      return new UninitialisedState<TItem>();
    }
  }
}