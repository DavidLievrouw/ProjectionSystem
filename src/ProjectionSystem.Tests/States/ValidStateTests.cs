using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using FakeItEasy;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class ValidStateTests {
    IProjectionSystem<Department> _projectionSystem;
    ISleeper _sleeper;
    TimeSpan _timeout;
    DeterministicTaskScheduler _taskScheduler;
    ValidState<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _projectionSystem = _projectionSystem.Fake();
      _timeout = TimeSpan.FromMilliseconds(200);
      _sleeper = _sleeper.Fake();
      _taskScheduler = new DeterministicTaskScheduler();
      _sut = new ValidState<Department>(_projectionSystem, _timeout, _sleeper, _taskScheduler);
    }

    [TestFixture]
    public class Construction : ValidStateTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        Assert.Throws<ArgumentNullException>(() => new ValidState<Department>(null, _timeout, _sleeper, _taskScheduler));
        Assert.Throws<ArgumentNullException>(() => new ValidState<Department>(_projectionSystem, _timeout, null, _taskScheduler));
        Assert.Throws<ArgumentNullException>(() => new ValidState<Department>(_projectionSystem, _timeout, _sleeper, null));
      }

      [Test]
      public void ZeroTimeout_Throws() {
        Assert.Throws<ArgumentException>(() => new ValidState<Department>(_projectionSystem, TimeSpan.Zero, _sleeper, _taskScheduler));
      }

      [Test]
      public void NegativeTimeout_Throws() {
        Assert.Throws<ArgumentException>(() => new ValidState<Department>(_projectionSystem, TimeSpan.FromSeconds(-1), _sleeper, _taskScheduler));
      }
    }

    [TestFixture]
    public class Id : ValidStateTests {
      [Test]
      public void ReturnsCorrectId() {
        Assert.That(_sut.Id, Is.EqualTo(StateId.Valid));
      }
    }

    [TestFixture]
    public class IsTransitionAllowed : ValidStateTests {
      [Test]
      public void PreviousStateIsRequired() {
        Assert.That(_sut.IsTransitionAllowed(null), Is.False);
      }

      [TestCase(StateId.Uninitialised)]
      [TestCase(StateId.Expired)]
      [TestCase(StateId.Valid)]
      public void NotAllowedToTransitionFromState(StateId id) {
        Assert.That(_sut.IsTransitionAllowed(id), Is.False);
      }

      [TestCase(StateId.Creating)]
      [TestCase(StateId.Updating)]
      public void AllowedToTransitionFromState(StateId id) {
        Assert.That(_sut.IsTransitionAllowed(id), Is.True);
      }
    }

    [TestFixture]
    public class BeforeEnter : ValidStateTests {
      IState<Department> _updatingState;
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _updatingState = _updatingState.Fake();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task KeepsTrackOfPreviousProjection() {
        A.CallTo(() => _projectionSystem.State)
          .Returns(_updatingState);
        A.CallTo(() => _updatingState.GetProjection())
          .Returns(_expectedProjection);

        await _sut.BeforeEnter();

        var currentProjection = await _sut.GetProjection();
        Assert.That(currentProjection, Is.EqualTo(_expectedProjection));
      }
    }

    [TestFixture]
    public class AfterEnter : ValidStateTests {
      TimeSpan _elapsedSleep;

      [SetUp]
      public override void SetUp() {
        base.SetUp();

        _elapsedSleep = TimeSpan.Zero;
        A.CallTo(() => _sleeper.Sleep(A<TimeSpan>._))
          .WithAnyArguments()
          .Invokes(fakeCall => _elapsedSleep += fakeCall.Arguments.Get<TimeSpan>(0))
          .Returns(Task.FromResult(true));
      }

      [Test]
      public void AfterDelay_MarksAsExpired() {
        var afterEnterTask = _sut.AfterEnter();
        _taskScheduler.RunTasksUntilIdle();

        A.CallTo(() => _sleeper.Sleep(_timeout)).MustHaveHappened()
          .Then(A.CallTo(() => _projectionSystem.InvalidateProjection()).MustHaveHappened());
        Assert.That(_elapsedSleep, Is.EqualTo(_timeout));
      }

      [Test]
      public void AfterDelay_MarksAsExpired_Asynchronously() {
        var afterEnterTask = _sut.AfterEnter();
        A.CallTo(() => _projectionSystem.InvalidateProjection()).MustNotHaveHappened();
      }
    }

    [TestFixture]
    public class GetProjection : ValidStateTests {
      IState<Department> _creatingState;
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _creatingState = _creatingState.Fake();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task ReturnsProjectionThatWasFetchedBefore() {
        // Make sure something is in the field
        A.CallTo(() => _projectionSystem.State)
          .Returns(_creatingState);
        A.CallTo(() => _creatingState.GetProjection())
          .Returns(_expectedProjection);
        await _sut.BeforeEnter();

        // Check if the method returns what is in the field
        var currentProjection = await _sut.GetProjection();
        Assert.That(currentProjection, Is.EqualTo(_expectedProjection));
      }
    }
  }
}