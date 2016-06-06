using System.Collections.Generic;

namespace ProjectionSystem {
  public interface IProjectionSystem {
    void TransitionToExpiredState();
    void TransitionToCreatingState();
    void TransitionToUpdatingState();
    void TransitionToCurrentState();
  }

  public interface IProjectionSystem<out TItem> : IProjectionSystem
    where TItem : IProjectedItem {
    IEnumerable<TItem> GetProjection();
  }
}