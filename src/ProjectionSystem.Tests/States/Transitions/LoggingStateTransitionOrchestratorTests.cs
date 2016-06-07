using System;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;
using ProjectionSystem.Diagnostics;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States.Transitions {
  [TestFixture]
  public class LoggingStateTransitionOrchestratorTests {
    IStateTransitionOrchestrator<Department> _inner;
    ITraceLogger _traceLogger;
    LoggingStateTransitionOrchestrator<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _inner = _inner.Fake();
      _traceLogger = _traceLogger.Fake();
      _sut = new LoggingStateTransitionOrchestrator<Department>(_inner, _traceLogger);
    }

    [TestFixture]
    public class Construction : LoggingStateTransitionOrchestratorTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class TransitionToState : LoggingStateTransitionOrchestratorTests {
      IState _state;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _state = _state.Fake();
      }

      [Test]
      public void GivenNullState_Throws() {
        Assert.ThrowsAsync<ArgumentNullException>(() => _sut.TransitionToState((IState)null));
      }

      [Test]
      public async Task DelegatesToInnerOrchestrator() {
        await _sut.TransitionToState(_state);
        A.CallTo(() => _inner.TransitionToState(_state)).MustHaveHappened();
      }

      [Test]
      public async Task LogsVerboseMessageBeforeTransitioning() {
        await _sut.TransitionToState(_state);
        A.CallTo(() => _traceLogger.Verbose(A<string>._)).MustHaveHappened()
          .Then(A.CallTo(() => _inner.TransitionToState(_state)).MustHaveHappened());
      }

      [Test]
      public async Task LogsVerboseMessageAfterTransitioning() {
        await _sut.TransitionToState(_state);
        A.CallTo(() => _inner.TransitionToState(_state)).MustHaveHappened()
          .Then(A.CallTo(() => _traceLogger.Verbose(A<string>._)).MustHaveHappened());
      }
    }

    [TestFixture]
    public class TransitionToStateOfTItem : LoggingStateTransitionOrchestratorTests {
      IState<Department> _state;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _state = _state.Fake();
      }

      [Test]
      public void GivenNullState_Throws() {
        Assert.ThrowsAsync<ArgumentNullException>(() => _sut.TransitionToState((IState<Department>)null));
      }

      [Test]
      public async Task DelegatesToInnerOrchestrator() {
        await _sut.TransitionToState(_state);
        A.CallTo(() => _inner.TransitionToState(_state)).MustHaveHappened();
      }

      [Test]
      public async Task LogsVerboseMessageBeforeTransitioning() {
        await _sut.TransitionToState(_state);
        A.CallTo(() => _traceLogger.Verbose(A<string>._)).MustHaveHappened()
          .Then(A.CallTo(() => _inner.TransitionToState(_state)).MustHaveHappened());
      }

      [Test]
      public async Task LogsVerboseMessageAfterTransitioning() {
        await _sut.TransitionToState(_state);
        A.CallTo(() => _inner.TransitionToState(_state)).MustHaveHappened()
          .Then(A.CallTo(() => _traceLogger.Verbose(A<string>._)).MustHaveHappened());
      }
    }

    [TestFixture]
    public class CurrentState : LoggingStateTransitionOrchestratorTests {
      [Test]
      public void ReturnsCurrentStateOfDecoratedOrchestrator() {
        var currentState = A.Fake<IState<Department>>();
        A.CallTo(() => _inner.CurrentState).Returns(currentState);
        var actual = _sut.CurrentState;
        Assert.That(actual, Is.EqualTo(currentState));
      }
    }
  }
}