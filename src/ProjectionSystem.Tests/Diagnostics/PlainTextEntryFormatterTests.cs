using System;
using System.Data;
using DavidLievrouw.Utils;
using FakeItEasy;
using NUnit.Framework;

namespace ProjectionSystem.Diagnostics {
  [TestFixture]
  public class PlainTextEntryFormatterTests {
    IExceptionFormatter _exceptionFormatter;
    PlainTextEntryFormatter _sut;

    [SetUp]
    public void SetUp() {
      _exceptionFormatter = A.Fake<IExceptionFormatter>();
      _sut = new PlainTextEntryFormatter(_exceptionFormatter);
    }

    [Test]
    public void ConstructorTests() {
      Assert.Throws<ArgumentNullException>(() => new PlainTextEntryFormatter(null));
    }

    [Test]
    public void GivenLogEntryIsNull_Throws() {
      Assert.Throws<ArgumentNullException>(() => _sut.Format(null));
    }

    [Test]
    public void GivenLogEntry_WithoutData_DoesNotThrow() {
      ConfigureExceptionStringFormatter_ToReturnSomethingValid();
      var input = new LogEntry(ConfigureSystemClockAmbientContext()) {
        EventId = 555,
        Severity = Severity.Warning
      };

      var actual = _sut.Format(input);

      Assert.That(string.IsNullOrEmpty(actual), Is.False);
      Assert.That(actual.Contains(input.EventId.ToString()), Is.True);
      Assert.That(actual.Contains(input.Severity.ToString()), Is.True);
    }

    [Test]
    public void GivenLogEntry_WithoutException_FormatsCorrectly() {
      ConfigureExceptionStringFormatter_ToReturnSomethingValid();
      var input = new LogEntry(ConfigureSystemClockAmbientContext()) {
        Data = "The log data.",
        EventId = 555,
        Severity = Severity.Warning
      };

      var actual = _sut.Format(input);

      Assert.That(string.IsNullOrEmpty(actual), Is.False);
      Assert.That(actual.Contains(input.Data.ToString()), Is.True);
      Assert.That(actual.Contains(input.EventId.ToString()), Is.True);
      Assert.That(actual.Contains(input.Severity.ToString()), Is.True);
      A.CallTo(() => _exceptionFormatter.Format(A<Exception>._))
        .MustNotHaveHappened();
    }

    [Test]
    public void GivenLogEntry_WithExceptionAndData_FormatsCorrectly() {
      ConfigureExceptionStringFormatter_ToReturnSomethingValid();
      var input = new LogEntry(ConfigureSystemClockAmbientContext()) {
        Data = "The log data.",
        EventId = 555,
        Severity = Severity.Warning,
        Exception = new InvalidConstraintException("Level 1", new Exception("Level 2"))
      };

      var actual = _sut.Format(input);

      Assert.That(string.IsNullOrEmpty(actual), Is.False);
      Assert.That(actual.Contains(input.Data.ToString()), Is.True);
      Assert.That(actual.Contains(input.EventId.ToString()), Is.True);
      Assert.That(actual.Contains(input.Severity.ToString()), Is.True);
      Assert.That(actual.Contains(FormattedExceptionString), Is.True);
      A.CallTo(() => _exceptionFormatter.Format(input.Exception))
        .MustHaveHappened();
    }

    const string FormattedExceptionString = "Some exception string.";
    void ConfigureExceptionStringFormatter_ToReturnSomethingValid() {
      A.CallTo(() => _exceptionFormatter.Format(A<Exception>._))
        .Returns(FormattedExceptionString);
    }

    static ISystemClock ConfigureSystemClockAmbientContext() {
      var systemClock = A.Fake<ISystemClock>();
      A.CallTo(() => systemClock.UtcNow).Returns(new DateTimeOffset(new DateTime(2015, 2, 9, 14, 00, 14, 123), TimeSpan.Zero));
      return systemClock;
    }
  }
}