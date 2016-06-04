using System.Collections.Generic;

namespace ProjectionSystem {
  public interface IProjectionDataService {
    void RefreshProjection();
  }

  public interface IProjectionDataService<out TItem> : IProjectionDataService
    where TItem : IProjectedItem {
    IEnumerable<TItem> GetProjection();
  }
}