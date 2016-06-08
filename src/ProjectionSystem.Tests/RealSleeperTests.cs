using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FluentAssertions;
using NUnit.Framework;

namespace ProjectionSystem {
  [TestFixture]
  public class RealSleeperTests {
    RealSleeper _sut;

    [SetUp]
    public virtual void SetUp() {
      _sut = new RealSleeper();
    }

    [TestFixture]
    public class Construction : RealSleeperTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class SleepByTimeSpanAndCancellationToken : RealSleeperTests {
      CancellationToken _cancellationToken;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _cancellationToken = new CancellationToken();
      }

      [Test]
      public void NegativeTimeout_Throws() {
        Assert.ThrowsAsync<ArgumentException>(() => _sut.Sleep(TimeSpan.FromSeconds(-50), _cancellationToken));
      }

      [TestCase(10)]
      [TestCase(50)]
      [TestCase(100)]
      [TestCase(200)]
      [TestCase(500)]
      [TestCase(1000)]
      public async Task SleepsForSpecifiedDuration(int millisecondsDuration) {
        var threadOverheadMargin = TimeSpan.FromMilliseconds(25);
        var duration = TimeSpan.FromMilliseconds(millisecondsDuration);
        var watch = Stopwatch.StartNew();
        await _sut.Sleep(duration, _cancellationToken);
        watch.Stop();
        Assert.That(watch.Elapsed, Is.GreaterThanOrEqualTo(duration));
        Assert.That(watch.Elapsed, Is.LessThanOrEqualTo(duration.Add(threadOverheadMargin)));
      }
    }

    [TestFixture]
    public class SleepByTimeSpan : RealSleeperTests {
      [Test]
      public void NegativeTimeout_Throws() {
        Assert.ThrowsAsync<ArgumentException>(() => _sut.Sleep(TimeSpan.FromSeconds(-50)));
      }

      [TestCase(10)]
      [TestCase(50)]
      [TestCase(100)]
      [TestCase(200)]
      [TestCase(500)]
      [TestCase(1000)]
      public async Task SleepsForSpecifiedDuration(int millisecondsDuration) {
        var threadOverheadMargin = TimeSpan.FromMilliseconds(25);
        var duration = TimeSpan.FromMilliseconds(millisecondsDuration);
        var watch = Stopwatch.StartNew();
        await _sut.Sleep(duration);
        watch.Stop();
        Assert.That(watch.Elapsed, Is.GreaterThanOrEqualTo(duration));
        Assert.That(watch.Elapsed, Is.LessThanOrEqualTo(duration.Add(threadOverheadMargin)));
      }
    }

    [TestFixture]
    public class SleepByMillisecondsAndCancellationToken : RealSleeperTests {
      CancellationToken _cancellationToken;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _cancellationToken = new CancellationToken();
      }

      [Test]
      public void NegativeTimeout_Throws() {
        Assert.ThrowsAsync<ArgumentException>(() => _sut.Sleep(-50, _cancellationToken));
      }

      [TestCase(10)]
      [TestCase(50)]
      [TestCase(100)]
      [TestCase(200)]
      [TestCase(500)]
      [TestCase(1000)]
      public async Task SleepsForSpecifiedDuration(int millisecondsDuration) {
        var threadOverheadMargin = 25;
        var watch = Stopwatch.StartNew();
        await _sut.Sleep(millisecondsDuration, _cancellationToken);
        watch.Stop();
        Assert.That(watch.Elapsed.TotalMilliseconds, Is.GreaterThanOrEqualTo(millisecondsDuration));
        Assert.That(watch.Elapsed.TotalMilliseconds, Is.LessThanOrEqualTo(millisecondsDuration + threadOverheadMargin));
      }
    }

    [TestFixture]
    public class SleepByMilliseconds : RealSleeperTests {
      [Test]
      public void NegativeTimeout_Throws() {
        Assert.ThrowsAsync<ArgumentException>(() => _sut.Sleep(-50));
      }

      [TestCase(10)]
      [TestCase(50)]
      [TestCase(100)]
      [TestCase(200)]
      [TestCase(500)]
      [TestCase(1000)]
      public async Task SleepsForSpecifiedDuration(int millisecondsDuration) {
        var threadOverheadMargin = 25;
        var watch = Stopwatch.StartNew();
        await _sut.Sleep(millisecondsDuration);
        watch.Stop();
        Assert.That(watch.Elapsed.TotalMilliseconds, Is.GreaterThanOrEqualTo(millisecondsDuration));
        Assert.That(watch.Elapsed.TotalMilliseconds, Is.LessThanOrEqualTo(millisecondsDuration + threadOverheadMargin));
      }
    }
  }
}