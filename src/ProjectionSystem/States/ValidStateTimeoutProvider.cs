using System;

namespace ProjectionSystem.States {
  public class ValidStateTimeoutProvider<TItem> : IValidStateTimeoutProvider<TItem>
    where TItem : IProjectedItem {
    readonly TimeSpan _timeout;

    public ValidStateTimeoutProvider(TimeSpan timeout) {
      if (timeout <= TimeSpan.Zero) throw new ArgumentException("An invalid projection timeout has been specified.", nameof(timeout));
      _timeout = timeout;
    }

    public TimeSpan ProvideTimeout() {
      return _timeout;
    }
  }
}