using System;
using System.Threading;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class UpdatingStateFactoryTests {
    IProjectionDataService<Department> _projectionDataService;
    ISyncLockFactory _syncLockFactory;
    TaskScheduler _taskScheduler;
    UpdatingStateFactory<Department> _sut;

    [OneTimeSetUp]
    public void OneTimeSetUp() {
      SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
    }

    [SetUp]
    public virtual void SetUp() {
      _projectionDataService = _projectionDataService.Fake();
      _syncLockFactory = _syncLockFactory.Fake();
      _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
      _sut = new UpdatingStateFactory<Department>(_projectionDataService, _syncLockFactory, _taskScheduler);
    }

    [TestFixture]
    public class Construction : UpdatingStateFactoryTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        Assert.Throws<ArgumentNullException>(() => new UpdatingStateFactory<Department>(_projectionDataService, _syncLockFactory, null));
        Assert.Throws<ArgumentNullException>(() => new UpdatingStateFactory<Department>(_projectionDataService, null, _taskScheduler));
        Assert.Throws<ArgumentNullException>(() => new UpdatingStateFactory<Department>(null, _syncLockFactory, _taskScheduler));
      }
    }

    [TestFixture]
    public class Create : UpdatingStateFactoryTests {
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
        Assert.That(actual, Is.InstanceOf<UpdatingState<Department>>());
      }
    }
  }
}