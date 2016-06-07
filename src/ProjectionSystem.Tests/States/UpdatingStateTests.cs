using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using FakeItEasy;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class UpdatingStateTests {
    IProjectionSystem<Department> _projectionSystem;
    IProjectionDataService<Department> _projectionDataService;
    ISyncLockFactory _syncLockFactory;
    DeterministicTaskScheduler _taskScheduler;
    UpdatingState<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _projectionSystem = _projectionSystem.Fake();
      _projectionDataService = _projectionDataService.Fake();
      _syncLockFactory = _syncLockFactory.Fake();
      _taskScheduler = new DeterministicTaskScheduler();
      _sut = new UpdatingState<Department>(_projectionSystem, _projectionDataService, _syncLockFactory, _taskScheduler);
    }

    [TestFixture]
    public class Construction : UpdatingStateTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        Assert.Throws<ArgumentNullException>(() => new UpdatingState<Department>(null, _projectionDataService, _syncLockFactory, _taskScheduler));
        Assert.Throws<ArgumentNullException>(() => new UpdatingState<Department>(_projectionSystem, null, _syncLockFactory, _taskScheduler));
        Assert.Throws<ArgumentNullException>(() => new UpdatingState<Department>(_projectionSystem, _projectionDataService, null, _taskScheduler));
        Assert.Throws<ArgumentNullException>(() => new UpdatingState<Department>(_projectionSystem, _projectionDataService, _syncLockFactory, null));
      }
    }

    [TestFixture]
    public class Id : UpdatingStateTests {
      [Test]
      public void ReturnsCorrectId() {
        Assert.That(_sut.Id, Is.EqualTo(StateId.Updating));
      }
    }

    [TestFixture]
    public class IsTransitionAllowed : UpdatingStateTests {
      [Test]
      public void PreviousStateIsRequired() {
        Assert.That(_sut.IsTransitionAllowed(null), Is.False);
      }

      [TestCase(StateId.Uninitialised)]
      [TestCase(StateId.Creating)]
      [TestCase(StateId.Updating)]
      [TestCase(StateId.Valid)]
      public void NotAllowedToTransitionFromState(StateId id) {
        Assert.That(_sut.IsTransitionAllowed(id), Is.False);
      }

      [TestCase(StateId.Expired)]
      public void AllowedToTransitionFromState(StateId id) {
        Assert.That(_sut.IsTransitionAllowed(id), Is.True);
      }
    }

    [TestFixture]
    public class BeforeEnter : UpdatingStateTests {
      IState<Department> _expiredState;
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _expiredState = _expiredState.Fake();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task KeepsTrackOfPreviousProjection() {
        A.CallTo(() => _projectionSystem.State)
          .Returns(_expiredState);
        A.CallTo(() => _expiredState.GetProjection())
          .Returns(_expectedProjection);

        await _sut.BeforeEnter();

        var currentProjection = await _sut.GetProjection();
        Assert.That(currentProjection, Is.EqualTo(_expectedProjection));
      }
    }

    [TestFixture]
    public class AfterEnter : UpdatingStateTests {
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task UpdatesAndCachesProjection_AndMarksAsUpToDate() {
        A.CallTo(() => _projectionDataService.GetProjection())
          .Returns(_expectedProjection);

        // Execute all tasks on the current thread and wait for idle
        var afterEnterTask = _sut.AfterEnter();
        _taskScheduler.RunTasksUntilIdle();

        A.CallTo(() => _projectionDataService.UpdateProjection()).MustHaveHappened()
          .Then(A.CallTo(() => _projectionDataService.GetProjection()).MustHaveHappened())
          .Then(A.CallTo(() => _projectionSystem.MarkProjectionAsUpToDate()).MustHaveHappened());

        var currentProjection = await _sut.GetProjection();
        Assert.That(currentProjection, Is.EqualTo(_expectedProjection));
      }

      [Test]
      public void UpdatesAndCachesProjection_AndMarksAsUpToDate_Asynchronously() {
        A.CallTo(() => _projectionDataService.GetProjection())
          .Returns(_expectedProjection);
        var afterEnterTask = _sut.AfterEnter();
        A.CallTo(() => _projectionDataService.UpdateProjection()).MustNotHaveHappened();
        A.CallTo(() => _projectionDataService.GetProjection()).MustNotHaveHappened();
        A.CallTo(() => _projectionSystem.MarkProjectionAsUpToDate()).MustNotHaveHappened();
      }

      [Test]
      public void UpdatesAndFetchesProjectionInLock_AndMarksAsUpToDateOutsideLock() {
        var hasLock = false;
        var isLockedDuringProjectionUpdating = false;
        var isLockedDuringProjectionFetching = false;
        var isLockedDuringMarkAsUpdated = false;

        var syncLock = A.Fake<ISyncLock>();
        A.CallTo(() => syncLock.Dispose())
          .Invokes(() => hasLock = false);
        A.CallTo(() => _syncLockFactory.Create())
          .Invokes(() => hasLock = true)
          .Returns(syncLock);

        A.CallTo(() => _projectionDataService.UpdateProjection())
          .Invokes(() => isLockedDuringProjectionUpdating = hasLock)
          .Returns(Task.FromResult(true));
        A.CallTo(() => _projectionDataService.GetProjection())
          .Invokes(() => isLockedDuringProjectionFetching = hasLock)
          .Returns(_expectedProjection);
        A.CallTo(() => _projectionSystem.MarkProjectionAsUpToDate())
          .Invokes(() => isLockedDuringMarkAsUpdated = hasLock)
          .Returns(Task.FromResult(true));

        var afterEnterTask = _sut.AfterEnter();
        _taskScheduler.RunTasksUntilIdle();

        Assert.That(isLockedDuringProjectionUpdating, Is.True, "The thread did not own the lock at the time of the projection updating.");
        Assert.That(isLockedDuringProjectionFetching, Is.True, "The thread did not own the lock at the time of the projection fetching.");
        Assert.That(isLockedDuringMarkAsUpdated, Is.False, "The thread did not release the lock before marking projection as up-to-date.");
        Assert.That(hasLock, Is.False, "The lock should be released after the call.");
      }
    }

    [TestFixture]
    public class GetProjection : UpdatingStateTests {
      IState<Department> _expiredState;
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _expiredState = _expiredState.Fake();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task ReturnsProjectionThatWasFetchedBefore() {
        // Make sure something is in the field
        A.CallTo(() => _projectionSystem.State)
          .Returns(_expiredState);
        A.CallTo(() => _expiredState.GetProjection())
          .Returns(_expectedProjection);
        await _sut.BeforeEnter();

        // Check if the method returns what is in the field
        var currentProjection = await _sut.GetProjection();
        Assert.That(currentProjection, Is.EqualTo(_expectedProjection));
      }
    }
  }
}