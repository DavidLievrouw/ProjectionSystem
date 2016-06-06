using System;

namespace ProjectionSystem.IntegrationTests.Items {
  public class Department : IProjectedItem {
    public int Id { get; set; }
    public Guid UniqueIdentifier { get; set; }
    public string Abbreviation { get; set; }
    public string Name { get; set; }
    public string PhoneNumber { get; set; }
    public PhoneType? PhoneType { get; set; }
    public string ContactName { get; set; }
    public DateTimeOffset ProjectionTime { get; set; }
  }
}