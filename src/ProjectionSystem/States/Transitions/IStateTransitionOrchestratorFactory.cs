namespace ProjectionSystem.States.Transitions {
  public interface IStateTransitionOrchestratorFactory<TItem> where TItem : IProjectedItem {
    IStateTransitionOrchestrator<TItem> CreateFor(IProjectionSystem<TItem> projectionSystem);
  }
}