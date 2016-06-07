using System;
using System.Threading;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class ValidStateFactoryTests {
    TimeSpan _timeout;
    TaskScheduler _taskScheduler;
    ValidStateFactory<Department> _sut;

    [OneTimeSetUp]
    public void OneTimeSetUp() {
      SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
    }

    [SetUp]
    public virtual void SetUp() {
      _timeout = TimeSpan.FromMinutes(1);
      _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
      _sut = new ValidStateFactory<Department>(_timeout, _taskScheduler);
    }

    [TestFixture]
    public class Construction : ValidStateFactoryTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        Assert.Throws<ArgumentNullException>(() => new ValidStateFactory<Department>(_timeout, null));
      }

      [Test]
      public void ZeroTimeout_Throws() {
        Assert.Throws<ArgumentException>(() => new ValidStateFactory<Department>(TimeSpan.Zero, _taskScheduler));
      }

      [Test]
      public void NegativeTimeout_Throws() {
        Assert.Throws<ArgumentException>(() => new ValidStateFactory<Department>(TimeSpan.FromSeconds(-1), _taskScheduler));
      }
    }

    [TestFixture]
    public class Create : ValidStateFactoryTests {
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
        Assert.That(actual, Is.InstanceOf<ValidState<Department>>());
      }
    }
  }
}