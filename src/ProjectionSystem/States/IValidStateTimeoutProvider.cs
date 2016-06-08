using System;

namespace ProjectionSystem.States {
  public interface IValidStateTimeoutProvider<TItem>
    where TItem : IProjectedItem {
    TimeSpan ProvideTimeout();
  }
}
