using System.Collections.Generic;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class CreatingStateTests {
    IProjectionSystem<Department> _projectionSystem;
    IProjectionDataService<Department> _projectionDataService;
    ISyncLockFactory _syncLockFactory;
    CreatingState<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _projectionSystem = _projectionSystem.Fake();
      _projectionDataService = _projectionDataService.Fake();
      _syncLockFactory = _syncLockFactory.Fake();
      _sut = new CreatingState<Department>(_projectionSystem, _projectionDataService, _syncLockFactory);
    }

    [TestFixture]
    public class Construction : CreatingStateTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class Id : CreatingStateTests {
      [Test]
      public void ReturnsCorrectId() {
        Assert.That(_sut.Id, Is.EqualTo(StateId.Creating));
      }
    }

    [TestFixture]
    public class IsTransitionAllowed : CreatingStateTests {
      [Test]
      public void PreviousStateIsRequired() {
        Assert.That(_sut.IsTransitionAllowed(null), Is.False);
      }

      [TestCase(StateId.Creating)]
      [TestCase(StateId.Expired)]
      [TestCase(StateId.Updating)]
      [TestCase(StateId.Valid)]
      public void NotAllowedToTransitionFromState(StateId id) {
        Assert.That(_sut.IsTransitionAllowed(id), Is.False);
      }

      [TestCase(StateId.Uninitialised)]
      public void AllowedToTransitionFromState(StateId id) {
        Assert.That(_sut.IsTransitionAllowed(id), Is.True);
      }
    }

    [TestFixture]
    public class BeforeEnter : CreatingStateTests {
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task UpdatesAndCachesProjection() {
        A.CallTo(() => _projectionDataService.GetProjection())
          .Returns(_expectedProjection);

        await _sut.BeforeEnter();

        A.CallTo(() => _projectionDataService.UpdateProjection()).MustHaveHappened()
          .Then(A.CallTo(() => _projectionDataService.GetProjection()).MustHaveHappened());

        var currentProjection = await _sut.GetProjection();
        Assert.That(currentProjection, Is.EqualTo(_expectedProjection));
      }

      [Test]
      public async Task UpdatesAndFetchesProjectionInLock() {
        var hasLock = false;
        var isLockedDuringProjectionUpdating = false;
        var isLockedDuringProjectionFetching = false;

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

        await _sut.BeforeEnter();

        Assert.That(isLockedDuringProjectionUpdating, Is.True, "The thread did not own the lock at the time of the projection updating.");
        Assert.That(isLockedDuringProjectionFetching, Is.True, "The thread did not own the lock at the time of the projection fetching.");
        Assert.That(hasLock, Is.False, "The lock should be released after the call.");
      }
    }

    [TestFixture]
    public class AfterEnter : CreatingStateTests {
      [Test]
      public async Task MarksProjectionAsUpdated() {
        await _sut.AfterEnter();
        A.CallTo(() => _projectionSystem.MarkProjectionAsUpToDate()).MustHaveHappened();
      }
    }

    [TestFixture]
    public class GetProjection : CreatingStateTests {
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task ReturnsProjectionThatWasFetchedInBeforeEnter() {
        A.CallTo(() => _projectionDataService.GetProjection())
          .Returns(_expectedProjection);
        await _sut.BeforeEnter();

        var currentProjection = await _sut.GetProjection();
        Assert.That(currentProjection, Is.EqualTo(_expectedProjection));
      }

      [Test]
      public async Task GetsProjectionInLock() {
        var hasLock = false;

        var syncLock = A.Fake<ISyncLock>();
        A.CallTo(() => syncLock.Dispose())
          .Invokes(() => hasLock = false);
        A.CallTo(() => _syncLockFactory.Create())
          .Invokes(() => hasLock = true)
          .Returns(syncLock);

        await _sut.GetProjection();

        A.CallTo(() => _syncLockFactory.Create()).MustHaveHappened()
          .Then(A.CallTo(() => syncLock.Dispose()).MustHaveHappened());
        Assert.That(hasLock, Is.False, "The lock should be released after the call.");
      }
    }
  }
}