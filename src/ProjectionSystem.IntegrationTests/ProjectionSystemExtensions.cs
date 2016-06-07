using System;
using System.Linq;
using System.Threading.Tasks;
using ProjectionSystem.IntegrationTests.Items;

namespace ProjectionSystem.IntegrationTests {
  public static class ProjectionSystemExtensions {
    public static async Task<DateTimeOffset?> GetLatestProjectionTime(this IProjectionSystem<Department> projectionSystem) {
      return projectionSystem == null
        ? new DateTimeOffset?()
        : (await projectionSystem.GetProjection().ConfigureAwait(false)).Max(dep => dep.ProjectionTime);
    }
  }
}