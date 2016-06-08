using System;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using FakeItEasy;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class ValidStateFactoryTests {
    ISleeper _sleeper;
    IValidStateTimeoutProvider<Department> _validStateTimeoutProvider;
    TaskScheduler _taskScheduler;
    ValidStateFactory<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _sleeper = _sleeper.Fake();
      _validStateTimeoutProvider = _validStateTimeoutProvider.Fake();
      _taskScheduler = new DeterministicTaskScheduler();
      _sut = new ValidStateFactory<Department>(_validStateTimeoutProvider, _sleeper, _taskScheduler);
    }

    [TestFixture]
    public class Construction : ValidStateFactoryTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        Assert.Throws<ArgumentNullException>(() => new ValidStateFactory<Department>(null, _sleeper, _taskScheduler));
        Assert.Throws<ArgumentNullException>(() => new ValidStateFactory<Department>(_validStateTimeoutProvider, null, _taskScheduler));
        Assert.Throws<ArgumentNullException>(() => new ValidStateFactory<Department>(_validStateTimeoutProvider, _sleeper, null));
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
      public void WhenTimeoutProviderReturnsInvalidTimeout_Throws() {
        A.CallTo(() => _validStateTimeoutProvider.ProvideTimeout()).Returns(TimeSpan.FromMinutes(-1));
        Assert.Throws<ArgumentException>(() => _sut.Create(_projectionSystem));
      }

      [Test]
      public void CreatesExpectedState() {
        A.CallTo(() => _validStateTimeoutProvider.ProvideTimeout()).Returns(TimeSpan.FromMinutes(1));
        var actual = _sut.Create(_projectionSystem);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.InstanceOf<ValidState<Department>>());
        A.CallTo(() => _validStateTimeoutProvider.ProvideTimeout()).MustHaveHappened();
      }
    }
  }
}