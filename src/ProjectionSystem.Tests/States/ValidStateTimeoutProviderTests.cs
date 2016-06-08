using System;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class ValidStateTimeoutProviderTests {
    TimeSpan _timeout;
    ValidStateTimeoutProvider<Department> _sut;

    [SetUp]
    public void SetUp() {
      _timeout = TimeSpan.FromMinutes(1);
      _sut = new ValidStateTimeoutProvider<Department>(_timeout);
    }

    [TestFixture]
    public class Construction : ValidStateTimeoutProviderTests {
      [Test]
      public void ZeroTimeout_Throws() {
        Assert.Throws<ArgumentException>(() => new ValidStateTimeoutProvider<Department>(TimeSpan.Zero));
      }

      [Test]
      public void NegativeTimeout_Throws() {
        Assert.Throws<ArgumentException>(() => new ValidStateTimeoutProvider<Department>(TimeSpan.FromSeconds(-1)));
      }
    }

    [TestFixture]
    public class ProvideTimeout : ValidStateTimeoutProviderTests {
      [Test]
      public void ReturnsTimeoutThatWasPassedDuringCreation() {
        Assert.That(_sut.ProvideTimeout(), Is.EqualTo(_timeout));
      }
    }
  }
}