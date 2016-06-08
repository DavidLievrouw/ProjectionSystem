using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem {
  public class RealSleeper : ISleeper {
    public Task Sleep(TimeSpan duration, CancellationToken cancellationToken) {
      if (duration <= TimeSpan.Zero) throw new ArgumentException("An invalid sleep duration has been specified.", nameof(duration));
      return Task.Delay(duration, cancellationToken);
    }

    public Task Sleep(TimeSpan duration) {
      return Sleep(duration, CancellationToken.None);
    }

    public Task Sleep(double millisecondsDuration) {
      return Sleep(TimeSpan.FromMilliseconds(millisecondsDuration), CancellationToken.None);
    }

    public Task Sleep(double millisecondsDuration, CancellationToken cancellationToken) {
      return Sleep(TimeSpan.FromMilliseconds(millisecondsDuration), cancellationToken);
    }
  }
}