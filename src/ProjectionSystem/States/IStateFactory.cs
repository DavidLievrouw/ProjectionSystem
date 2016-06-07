namespace ProjectionSystem.States {
  public interface IStateFactory<TItem>
    where TItem : IProjectedItem {
    IState<TItem> Create(IProjectionSystem<TItem> projectionSystem);
  }
}