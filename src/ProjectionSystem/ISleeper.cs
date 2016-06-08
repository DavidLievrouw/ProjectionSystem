using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectionSystem {
  public interface ISleeper {
    Task Sleep(TimeSpan duration);
    Task Sleep(TimeSpan duration, CancellationToken cancellationToken);
    Task Sleep(double millisecondsDuration);
    Task Sleep(double millisecondsDuration, CancellationToken cancellationToken);
  }
}
