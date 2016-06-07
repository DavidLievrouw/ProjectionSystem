namespace ProjectionSystem.States {
  public class UninitialisedStateFactory<TItem> : IStateFactory<TItem>
    where TItem : IProjectedItem {

    public IState<TItem> Create(IProjectionSystem<TItem> projectionSystem) {
      return new UninitialisedState<TItem>();
    }
  }
}