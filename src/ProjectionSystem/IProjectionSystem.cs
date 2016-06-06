using System.Collections.Generic;
using ProjectionSystem.States;

namespace ProjectionSystem {
  public interface IProjectionSystem {
    void TransitionToExpiredState();
    void TransitionToCreatingState();
    void TransitionToUpdatingState();
    void TransitionToCurrentState();
    IState State { get; }
  }

  public interface IProjectionSystem<out TItem> : IProjectionSystem
    where TItem : IProjectedItem {
    IEnumerable<TItem> GetProjection();
  }
}