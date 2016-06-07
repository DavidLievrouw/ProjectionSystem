using System;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FluentAssertions;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class CreatingStateFactoryTests {
    IProjectionDataService<Department> _projectionDataService;
    ISyncLockFactory _syncLockFactory;
    CreatingStateFactory<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _projectionDataService = _projectionDataService.Fake();
      _syncLockFactory = _syncLockFactory.Fake();
      _sut = new CreatingStateFactory<Department>(_projectionDataService, _syncLockFactory);
    }

    [TestFixture]
    public class Construction : CreatingStateFactoryTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class Create : CreatingStateFactoryTests {
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
        Assert.That(actual, Is.InstanceOf<CreatingState<Department>>());
      }
    }
  }
}