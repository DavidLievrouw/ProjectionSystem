using System;

namespace ProjectionSystem {
  public interface IProjectedItem {
    DateTimeOffset ProjectionTime { get; }
  }
}
