using System;
using DavidLievrouw.Utils;
using FakeItEasy;
using NUnit.Framework;

namespace ProjectionSystem.Diagnostics {
  [TestFixture]
  public class LogEntryTests {
    [Test]
    public void ConstructorSetsReadOnlyProperties() {
      var sut = new LogEntry(ConfigureFixedSystemClock());

      Assert.That(sut.DateTimeUtc, Is.EqualTo(new DateTimeOffset(new DateTime(2015, 2, 9, 14, 00, 14, 123), TimeSpan.Zero)));
      Assert.That(sut.MachineName, Is.EqualTo(Environment.MachineName));
      Assert.That(sut.User, Is.EqualTo(Environment.UserDomainName + "\\" + Environment.UserName));
      Assert.That(sut.Version, Is.EqualTo(GetType().Assembly.GetName().Version));
      Assert.That(sut.EventId, Is.EqualTo(0));
      Assert.That(sut.Severity, Is.EqualTo(Severity.Verbose));
    }

    static ISystemClock ConfigureFixedSystemClock() {
      var systemClock = A.Fake<ISystemClock>();
      A.CallTo(() => systemClock.UtcNow).Returns(new DateTimeOffset(new DateTime(2015, 2, 9, 14, 00, 14, 123), TimeSpan.Zero));
      return systemClock;
    }
  }
}