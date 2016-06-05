using System;
using System.Diagnostics;
using NUnit.Framework;

namespace ProjectionSystem.Diagnostics {
  [TestFixture]
  public class TraceEventTypeAdapterTests {
    TraceEventTypeAdapter _sut;

    [SetUp]
    public void SetUp() {
      _sut = new TraceEventTypeAdapter();
    }

    [Test]
    public void GivenInvalidSeverity_Throws() {
      const Severity invalidSeverity = (Severity)(-111);
      Assert.Throws<ArgumentOutOfRangeException>(() => _sut.Adapt(invalidSeverity));
    }

    [TestCase(Severity.Information, TraceEventType.Information)]
    [TestCase(Severity.Verbose, TraceEventType.Verbose)]
    [TestCase(Severity.Warning, TraceEventType.Warning)]
    [TestCase(Severity.Error, TraceEventType.Error)]
    [TestCase(Severity.Critical, TraceEventType.Critical)]
    public void GivenValidSeverity_AdaptsCorrectly(Severity input, TraceEventType expectedResult) {
      var actual = _sut.Adapt(input);
      Assert.That(actual, Is.EqualTo(expectedResult));
    }
  }
}