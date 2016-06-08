using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem {
  public class TestSleeper : ISleeper {
    public Task Sleep(TimeSpan duration, CancellationToken cancellationToken) {
      if (duration <= TimeSpan.Zero) throw new ArgumentException("An invalid sleep duration has been specified.", nameof(duration));
      TotalSleepElapsed += duration;
      return Task.FromResult(true);
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

    public TimeSpan TotalSleepElapsed { get; private set; }
  }
}
