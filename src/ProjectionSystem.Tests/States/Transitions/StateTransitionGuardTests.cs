using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace ProjectionSystem.States.Transitions {
  [TestFixture]
  public class StateTransitionGuardTests {
    IState _state;
    StateTransitionGuard _sut;

    [SetUp]
    public virtual void SetUp() {
      _state = _state.Fake();
      _sut = new StateTransitionGuard(_state);
    }

    [TestFixture]
    public class Construction : StateTransitionGuardTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class StateTransitionAllowed : StateTransitionGuardTests {
      IState _previousState;
      StateId _previousStateId;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _previousState = _previousState.Fake();
        _previousStateId = StateId.Expired;
        A.CallTo(() => _previousState.Id).Returns(_previousStateId);
      }

      [Test]
      public void GivenNullPreviousState_AndThatsAllowed_DoesNotThrow() {
        _previousState = null;
        A.CallTo(() => _state.IsTransitionAllowed(null)).Returns(true);
        Assert.DoesNotThrow(() => _sut.StateTransitionAllowed(_previousState));
      }

      [Test]
      public void GivenNullPreviousState_AndThatsNotAllowed_Throws() {
        _previousState = null;
        A.CallTo(() => _state.IsTransitionAllowed(null)).Returns(false);
        Assert.Throws<InvalidStateTransitionException>(() => _sut.StateTransitionAllowed(_previousState));
      }

      [Test]
      public void IfTransitionIsNotAllowed_Throws() {
        A.CallTo(() => _state.IsTransitionAllowed(_previousStateId)).Returns(false);
        Assert.Throws<InvalidStateTransitionException>(() => _sut.StateTransitionAllowed(_previousState));
      }

      [Test]
      public void IfTransitionIsAllowed_DoesNotThrow() {
        A.CallTo(() => _state.IsTransitionAllowed(_previousStateId)).Returns(true);
        Assert.DoesNotThrow(() => _sut.StateTransitionAllowed(_previousState));
      }
    }
  }
}