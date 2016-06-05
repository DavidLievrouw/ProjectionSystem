using System;

namespace ProjectionSystem.Diagnostics {
  public interface IExceptionFormatter {
    string Format(Exception exception);
  }
}