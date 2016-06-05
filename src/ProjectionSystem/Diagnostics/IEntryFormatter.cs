namespace ProjectionSystem.Diagnostics {
  public interface IEntryFormatter {
    string Format(ILogEntry entry);
  }
}