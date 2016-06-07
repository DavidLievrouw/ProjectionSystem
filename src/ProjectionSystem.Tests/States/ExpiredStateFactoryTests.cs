using System;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FluentAssertions;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class ExpiredStateFactoryTests {
    ExpiredStateFactory<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _sut = new ExpiredStateFactory<Department>();
    }

    [TestFixture]
    public class Construction : ExpiredStateFactoryTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class Create : ExpiredStateFactoryTests {
      IProjectionSystem<Department> _projectionSystem;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _projectionSystem = _projectionSystem.Fake();
      }

      [Test]
      public void GivenNullProjectionSystem_Throws() {
        Assert.Throws<ArgumentNullException>(() => _sut.Create(null));
      }

      [Test]
      public void CreatesExpectedState() {
        var actual = _sut.Create(_projectionSystem);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.InstanceOf<ExpiredState<Department>>());
      }
    }
  }
}