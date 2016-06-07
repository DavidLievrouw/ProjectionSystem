using System;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States.Transitions {
  [TestFixture]
  public class StateTransitionOrchestratorTests {
    IStateTransitionGuardFactory _stateTransitionGuardFactory;
    StateTransitionOrchestrator<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _stateTransitionGuardFactory = _stateTransitionGuardFactory.Fake();
      _sut = new StateTransitionOrchestrator<Department>(_stateTransitionGuardFactory);
    }

    [TestFixture]
    public class Construction : StateTransitionOrchestratorTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class TransitionToState : StateTransitionOrchestratorTests {
      IState _state;
      IStateTransitionGuard _transitionGuard;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _state = A.Fake<IState<Department>>();
        _transitionGuard = _transitionGuard.Fake();
        A.CallTo(() => _stateTransitionGuardFactory.CreateFor(_state)).Returns(_transitionGuard);
      }

      [Test]
      public void GivenNullState_Throws() {
        Assert.ThrowsAsync<ArgumentNullException>(() => _sut.TransitionToState((IState)null));
      }

      [Test]
      public void GivenStateThatIsNotOfCompatibleType_Throws() {
        var invalidlyTypedState = A.Fake<IState<FakeProjectedItem>>();
        Assert.ThrowsAsync<ArgumentException>(() => _sut.TransitionToState((IState)invalidlyTypedState));
      }

      [Test]
      public async Task CreatesAndCallsTransitionGuard() {
        await _sut.TransitionToState(_state);
        A.CallTo(() => _transitionGuard.StateTransitionAllowed(A<IState>._)).MustHaveHappened();
      }

      [Test]
      public void WhenTransitionGuardThrows_Throws_DoesNotChangeState() {
        A.CallTo(() => _transitionGuard.StateTransitionAllowed(A<IState>._))
          .Throws(new IndexOutOfRangeException());
        Assert.ThrowsAsync<IndexOutOfRangeException>(() => _sut.TransitionToState(_state));
        Assert.That(_sut.CurrentState, Is.Null);
      }

      [Test]
      public async Task CallsBeforeEnter_ThenSetsCurrentState_ThenCallsAfterEnter() {
        IState stateOnBeforeEnter = null;
        IState stateOnAfterEnter = null;
        A.CallTo(() => _state.BeforeEnter())
          .Invokes(fakeCall => stateOnBeforeEnter = _sut.CurrentState)
          .Returns(Task.FromResult(true));
        A.CallTo(() => _state.AfterEnter())
          .Invokes(fakeCall => stateOnAfterEnter = _sut.CurrentState)
          .Returns(Task.FromResult(true));

        await _sut.TransitionToState(_state);

        Assert.That(stateOnBeforeEnter, Is.Null); // Initial state value is null
        Assert.That(stateOnAfterEnter, Is.EqualTo(_state));
        A.CallTo(() => _state.BeforeEnter()).MustHaveHappened()
          .Then(A.CallTo(() => _state.AfterEnter()).MustHaveHappened());
      }

      public class FakeProjectedItem : IProjectedItem {
        public DateTimeOffset ProjectionTime { get; }
      }
    }

    [TestFixture]
    public class TransitionToStateOfTItem : StateTransitionOrchestratorTests {
      IState<Department> _state;
      IStateTransitionGuard _transitionGuard;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _state = _state.Fake();
        _transitionGuard = _transitionGuard.Fake();
        A.CallTo(() => _stateTransitionGuardFactory.CreateFor(_state)).Returns(_transitionGuard);
      }

      [Test]
      public void GivenNullState_Throws() {
        Assert.ThrowsAsync<ArgumentNullException>(() => _sut.TransitionToState((IState<Department>)null));
      }

      [Test]
      public async Task CreatesAndCallsTransitionGuard() {
        await _sut.TransitionToState(_state);
        A.CallTo(() => _transitionGuard.StateTransitionAllowed(A<IState<Department>>._)).MustHaveHappened();
      }

      [Test]
      public void WhenTransitionGuardThrows_Throws_DoesNotChangeState() {
        A.CallTo(() => _transitionGuard.StateTransitionAllowed(A<IState<Department>>._))
          .Throws(new IndexOutOfRangeException());
        Assert.ThrowsAsync<IndexOutOfRangeException>(() => _sut.TransitionToState(_state));
        Assert.That(_sut.CurrentState, Is.Null);
      }

      [Test]
      public async Task CallsBeforeEnter_ThenSetsCurrentState_ThenCallsAfterEnter() {
        IState stateOnBeforeEnter = null;
        IState stateOnAfterEnter = null;
        A.CallTo(() => _state.BeforeEnter())
          .Invokes(fakeCall => stateOnBeforeEnter = _sut.CurrentState)
          .Returns(Task.FromResult(true));
        A.CallTo(() => _state.AfterEnter())
          .Invokes(fakeCall => stateOnAfterEnter = _sut.CurrentState)
          .Returns(Task.FromResult(true));

        await _sut.TransitionToState(_state);

        Assert.That(stateOnBeforeEnter, Is.Null); // Initial state value is null
        Assert.That(stateOnAfterEnter, Is.EqualTo(_state));
        A.CallTo(() => _state.BeforeEnter()).MustHaveHappened()
          .Then(A.CallTo(() => _state.AfterEnter()).MustHaveHappened());
      }
    }

    [TestFixture]
    public class CurrentState : StateTransitionOrchestratorTests {
      [Test]
      public async Task StateThatWasLastTransitionedTo() {
        var currentState = A.Fake<IState<Department>>();
        await _sut.TransitionToState(currentState);
        var actual = _sut.CurrentState;
        Assert.That(actual, Is.EqualTo(currentState));
      }
    }
  }
}