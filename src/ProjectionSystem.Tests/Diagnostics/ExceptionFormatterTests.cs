using System;
using System.Data;
using System.IO;
using FluentAssertions;
using NUnit.Framework;

namespace ProjectionSystem.Diagnostics {
  [TestFixture]
  public class ExceptionFormatterTests {
    ExceptionFormatter _sut;

    [SetUp]
    public void SetUp() {
      _sut = new ExceptionFormatter();
    }

    [Test]
    public void GivenExceptionIsNull_ReturnsEmptyString() {
      var actual = _sut.Format(null);
      Assert.That(actual, Is.EqualTo(string.Empty));
    }

    [Test]
    public void SimpleException_ReturnsExpectedResult() {
      var simpleException = new InvalidDataException("Some error!");

      var actual = _sut.Format(simpleException);

      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.Contains("System.IO.InvalidDataException"), Is.True);
      Assert.That(actual.Contains("Some error!"), Is.True);
      Assert.That(actual.Contains("Stacktrace: {NULL}"), Is.True);
    }

    [Test]
    public void ComplexException_ReturnsExpectedResult() {
      var complexException = new InvalidDataException("Some error!",
        new InvalidConstraintException("Some inner error.", new Exception("Some really inner error.")));

      var actual = _sut.Format(complexException);

      Assert.That(actual, Is.Not.Null);
      Assert.That(actual.Contains("System.IO.InvalidDataException"), Is.True);
      Assert.That(actual.Contains("Some error!"), Is.True);
      Assert.That(actual.Contains("Some inner error."), Is.True);
      Assert.That(actual.Contains("Some really inner error."), Is.True);
      Assert.That(actual.Contains("Stacktrace: {NULL}"), Is.True);
    }

    [Test]
    public void ComplexExceptionWithStackTrace_ReturnsExpectedResult() {
      var complexException = new InvalidDataException("Some error!",
        new InvalidConstraintException("Some inner error.", new Exception("Some really inner error.")));

      string actual = null;
      try {
        throw complexException;
      } catch (Exception ex) {
        actual = _sut.Format(ex);
      }
      actual.Should().NotBeNullOrEmpty()
        .And.Contain("System.IO.InvalidDataException")
        .And.Contain("Some error!")
        .And.Contain("Some inner error.")
        .And.Contain("Some really inner error.")
        .And.Contain("   at ProjectionSystem.Diagnostics.ExceptionFormatterTests");
    }
  }
}