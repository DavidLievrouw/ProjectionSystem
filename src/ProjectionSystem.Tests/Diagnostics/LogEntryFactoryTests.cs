using DavidLievrouw.Utils;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using NUnit.Framework;

namespace ProjectionSystem.Diagnostics {
  [TestFixture]
  public class LogEntryFactoryTests {
    LogEntryFactory _sut;
    ISystemClock _systemClock;

    [SetUp]
    public void SetUp() {
      _systemClock = _systemClock.Fake();
      _sut = new LogEntryFactory(_systemClock);
    }

    [Test]
    public void CreatesEmptyLogEntry() {
      var expected = new LogEntry(_systemClock);
      var actual = _sut.Create();
      Assert.That(actual, Is.Not.Null);
    }
  }
}