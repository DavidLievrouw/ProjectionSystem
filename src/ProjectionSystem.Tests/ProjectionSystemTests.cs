using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FakeItEasy;
using FakeItEasy.Configuration;
using FluentAssertions;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;
using ProjectionSystem.States;
using ProjectionSystem.States.Transitions;

namespace ProjectionSystem {
  [TestFixture]
  public class ProjectionSystemTests {
    IStateTransitionOrchestrator<Department> _stateTransitionOrchestrator;
    IStateFactory<Department> _uninitialisedStateFactory;
    IStateFactory<Department> _creatingStateFactory;
    IStateFactory<Department> _validStateFactory;
    IStateFactory<Department> _expiredStateFactory;
    IStateFactory<Department> _updatingStateFactory;
    ISyncLockFactory _getProjectionLockFactory;
    ProjectionSystem<Department> _sut;
    IState<Department> _uninitialisedState;
    IState<Department> _creatingState;
    IState<Department> _validState;
    IState<Department> _expiredState;
    IState<Department> _updatingState;
    IReturnValueArgumentValidationConfiguration<Task> _initialiseStateCall;
    Task _initialiseStateTask;

    [SetUp]
    public virtual void SetUp() {
      _stateTransitionOrchestrator = _stateTransitionOrchestrator.Fake();
      _uninitialisedStateFactory = _uninitialisedStateFactory.Fake();
      _creatingStateFactory = _creatingStateFactory.Fake();
      _validStateFactory = _validStateFactory.Fake();
      _expiredStateFactory = _expiredStateFactory.Fake();
      _updatingStateFactory = _updatingStateFactory.Fake();
      _getProjectionLockFactory = _getProjectionLockFactory.Fake();

      _uninitialisedState = _uninitialisedState.Fake();
      _creatingState = _creatingState.Fake();
      _validState = _validState.Fake();
      _expiredState = _expiredState.Fake();
      _updatingState = _updatingState.Fake();

      _initialiseStateCall = A.CallTo(() => _stateTransitionOrchestrator.TransitionToState(A<IState<Department>>.That.Matches(state => state.Id == StateId.Uninitialised)));
      _initialiseStateTask = new Task(() => {});
      _initialiseStateCall.Returns(_initialiseStateTask);

      ConfigureStateFactory_ToReturnState(_uninitialisedStateFactory, _uninitialisedState);
      ConfigureStateFactory_ToReturnState(_creatingStateFactory, _creatingState);
      ConfigureStateFactory_ToReturnState(_validStateFactory, _validState);
      ConfigureStateFactory_ToReturnState(_expiredStateFactory, _expiredState);
      ConfigureStateFactory_ToReturnState(_updatingStateFactory, _updatingState);

      ConfigureState_ToHaveStateId(_uninitialisedState, StateId.Uninitialised);
      ConfigureState_ToHaveStateId(_creatingState, StateId.Creating);
      ConfigureState_ToHaveStateId(_validState, StateId.Valid);
      ConfigureState_ToHaveStateId(_expiredState, StateId.Expired);
      ConfigureState_ToHaveStateId(_updatingState, StateId.Updating);

      _sut = new ProjectionSystem<Department>(
        _stateTransitionOrchestrator,
        _uninitialisedStateFactory,
        _creatingStateFactory,
        _validStateFactory,
        _expiredStateFactory,
        _updatingStateFactory,
        _getProjectionLockFactory);
    }

    [TestFixture]
    public class Construction : ProjectionSystemTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }

      [Test]
      public void OnConstruction_ShouldEnterUninitialisedState() {
        _initialiseStateCall.MustHaveHappened();
        Assert.That(_initialiseStateTask.IsCompleted);
      }
      
      [Test]
      public void OnConstruction_AndWhenInitialisationTaskHasBeenCompleted_DoesNotThrow() {
        var initialiseStateCall = A.CallTo(() => _stateTransitionOrchestrator.TransitionToState(A<IState<Department>>.That.Matches(state => state.Id == StateId.Uninitialised)));
        var completedInitialisationTask = Task.FromResult(true);
        initialiseStateCall.Returns(completedInitialisationTask);

        Assert.DoesNotThrow(() => new ProjectionSystem<Department>(
          _stateTransitionOrchestrator,
          _uninitialisedStateFactory,
          _creatingStateFactory,
          _validStateFactory,
          _expiredStateFactory,
          _updatingStateFactory,
          _getProjectionLockFactory));

        Assert.That(completedInitialisationTask.IsCompleted);
      }

      [Test]
      public void OnConstruction_ShouldEnterUninitialisedState_EvenIfItRunsALongTime() {
        var isFinished = false;
        var initialiseStateCall = A.CallTo(() => _stateTransitionOrchestrator.TransitionToState(A<IState<Department>>.That.Matches(state => state.Id == StateId.Uninitialised)));
        var initialisationTask = new Task(() => {
          Thread.Sleep(250); // Long running thing
          isFinished = true;
        }, TaskCreationOptions.LongRunning);
        initialiseStateCall.Returns(initialisationTask);

        new ProjectionSystem<Department>(
          _stateTransitionOrchestrator,
          _uninitialisedStateFactory,
          _creatingStateFactory,
          _validStateFactory,
          _expiredStateFactory,
          _updatingStateFactory,
          _getProjectionLockFactory);

        Assert.That(isFinished, Is.True);
        Assert.That(initialisationTask.IsCompleted);
      }
    }

    [TestFixture]
    public class InvalidateProjection : ProjectionSystemTests {
      [Test]
      public async Task TriggersTransitionToExpiredState() {
        await _sut.InvalidateProjection();
        A.CallTo(() => _stateTransitionOrchestrator.TransitionToState(_expiredState)).MustHaveHappened();
      }
    }

    [TestFixture]
    public class MarkProjectionAsUpToDate : ProjectionSystemTests {
      [Test]
      public async Task TriggersTransitionToValidState() {
        await _sut.MarkProjectionAsUpToDate();
        A.CallTo(() => _stateTransitionOrchestrator.TransitionToState(_validState)).MustHaveHappened();
      }
    }

    [TestFixture]
    public class GetProjection : ProjectionSystemTests {
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task WhenStateIsUninitialised_TransitionsToCreatingStateFirst() {
        var currentState = _uninitialisedState;
        A.CallTo(() => _stateTransitionOrchestrator.CurrentState).ReturnsLazily(() => currentState);
        A.CallTo(() => _creatingState.GetProjection()).Returns(_expectedProjection);
        A.CallTo(() => _stateTransitionOrchestrator.TransitionToState(_creatingState))
          .Invokes(fakeCall => currentState = _creatingState)
          .Returns(Task.FromResult(true));

        await _sut.GetProjection();

        var actual = await _sut.GetProjection();
        Assert.That(actual, Is.EqualTo(_expectedProjection));
        A.CallTo(() => _stateTransitionOrchestrator.TransitionToState(_creatingState)).MustHaveHappened()
          .Then(A.CallTo(() => _creatingState.GetProjection()).MustHaveHappened());
      }

      [Test]
      public async Task WhenStateIsExpired_TransitionsToUpdatingStateFirst() {
        var currentState = _expiredState;
        A.CallTo(() => _stateTransitionOrchestrator.CurrentState).ReturnsLazily(() => currentState);
        A.CallTo(() => _updatingState.GetProjection()).Returns(_expectedProjection);
        A.CallTo(() => _stateTransitionOrchestrator.TransitionToState(_updatingState))
          .Invokes(fakeCall => currentState = _updatingState)
          .Returns(Task.FromResult(true));

        await _sut.GetProjection();

        var actual = await _sut.GetProjection();
        Assert.That(actual, Is.EqualTo(_expectedProjection));
        A.CallTo(() => _stateTransitionOrchestrator.TransitionToState(_updatingState)).MustHaveHappened()
          .Then(A.CallTo(() => _updatingState.GetProjection()).MustHaveHappened());
      }

      [Test]
      public async Task ReturnsProjectionFromState() {
        A.CallTo(() => _stateTransitionOrchestrator.CurrentState).Returns(_updatingState);
        A.CallTo(() => _updatingState.GetProjection()).Returns(_expectedProjection);
        var actual = await _sut.GetProjection();
        Assert.That(actual, Is.EqualTo(_expectedProjection));
      }
    }

    [TestFixture]
    public class State : ProjectionSystemTests {
      [Test]
      public void ReturnsStateFromOrchestrator() {
        var expected = _updatingState;
        A.CallTo(() => _stateTransitionOrchestrator.CurrentState).Returns(expected);
        var actual = _sut.State;
        Assert.That(actual, Is.EqualTo(expected));
      }
    }

    void ConfigureState_ToHaveStateId(IState state, StateId stateId) {
      A.CallTo(() => state.Id).Returns(stateId);
    }

    void ConfigureStateFactory_ToReturnState(IStateFactory<Department> stateFactory, IState<Department> state) {
      A.CallTo(() => stateFactory.Create(A<IProjectionSystem<Department>>._)).Returns(state);
    }
  }
}