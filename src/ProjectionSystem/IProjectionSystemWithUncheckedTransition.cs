using System.Threading.Tasks;

namespace ProjectionSystem {
  internal interface IProjectionSystemWithUncheckedTransition<TItem> : IProjectionSystem<TItem>
    where TItem : IProjectedItem {
    Task TransitionToCurrentStateUnchecked();
  }
}