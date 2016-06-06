using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectionSystem {
  public interface IProjectionDataService {
    Task UpdateProjection();
  }

  public interface IProjectionDataService<TItem> : IProjectionDataService
    where TItem : IProjectedItem {
    Task<IEnumerable<TItem>> GetProjection();
  }
}