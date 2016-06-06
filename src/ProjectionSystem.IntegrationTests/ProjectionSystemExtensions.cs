using System;
using System.Linq;

namespace ProjectionSystem.IntegrationTests {
  public static class ProjectionSystemExtensions {
    public static DateTimeOffset? GetLatestProjectionTime(this IProjectionSystem<IProjectedItem> projectionSystem) {
      return projectionSystem?.GetProjection().Max(dep => dep.ProjectionTime);
    }
  }
}