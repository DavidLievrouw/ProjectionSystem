using System;
using System.Diagnostics;
using DavidLievrouw.Utils;
using FakeItEasy;
using NUnit.Framework;

namespace ProjectionSystem.Diagnostics {
  [TestFixture]
  public class TraceLoggerTests {
    const string SourceName = "DavidLievrouw:TraceLogger";
    IAdapter<Severity, TraceEventType> _traceEventTypeAdapter;
    IEntryFormatter _entryFormatter;
    ILogEntryFactory _logEntryFactory;
    TraceLogger _sut;

    [SetUp]
    public void SetUp() {
      _traceEventTypeAdapter = A.Fake<IAdapter<Severity, TraceEventType>>();
      _entryFormatter = A.Fake<IEntryFormatter>();
      _logEntryFactory = A.Fake<ILogEntryFactory>();
      _sut = new TraceLogger(SourceName, _traceEventTypeAdapter, _entryFormatter, _logEntryFactory);
    }

    [Test]
    public void ConstructorTests() {
      Assert.Throws<ArgumentNullException>(() => new TraceLogger(null, _traceEventTypeAdapter, _entryFormatter, _logEntryFactory));
      Assert.Throws<ArgumentNullException>(() => new TraceLogger(SourceName, null, _entryFormatter, _logEntryFactory));
      Assert.Throws<ArgumentNullException>(() => new TraceLogger(SourceName, _traceEventTypeAdapter, null, _logEntryFactory));
      Assert.Throws<ArgumentNullException>(() => new TraceLogger(SourceName, _traceEventTypeAdapter, _entryFormatter, null));
    }

    [Test]
    public void GivenNullLogEntry_Throws() {
      Assert.Throws<ArgumentNullException>(() => _sut.Log(null));
    }

    [Test]
    public void ValidLogEntry_Maps_Formats_DoesNotThrow() {
      var input = new LogEntry(new RealSystemClock()) {
        Severity = Severity.Warning
      };

      Assert.DoesNotThrow(() => _sut.Log(input));

      A.CallTo(() => _traceEventTypeAdapter.Adapt(Severity.Warning)).MustHaveHappened();
      A.CallTo(() => _entryFormatter.Format(input)).MustHaveHappened();
    }

    [Test]
    public void WhenUsingConvenienceMethod_UsesLogEntryFactory() {
      Assert.DoesNotThrow(() => _sut.Log(Severity.Error, "Some dummy data"));
      A.CallTo(() => _logEntryFactory.Create()).MustHaveHappened();
    }
  }
}