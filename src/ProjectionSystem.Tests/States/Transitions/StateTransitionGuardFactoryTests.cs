using System;
using DavidLievrouw.Utils.ForTesting.FakeItEasy;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FluentAssertions;
using NUnit.Framework;

namespace ProjectionSystem.States.Transitions {
  [TestFixture]
  public class StateTransitionGuardFactoryTests {
    StateTransitionGuardFactory _sut;

    [SetUp]
    public virtual void SetUp() {
      _sut = new StateTransitionGuardFactory();
    }

    [TestFixture]
    public class Construction : StateTransitionGuardFactoryTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class Create : StateTransitionGuardFactoryTests {
      IState _state;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _state = _state.Fake();
      }

      [Test]
      public void GivenNullState_Throws() {
        Assert.Throws<ArgumentNullException>(() => _sut.CreateFor(null));
      }

      [Test]
      public void CreatesStateTransitionGuard() {
        var actual = _sut.CreateFor(_state);
        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Is.InstanceOf<StateTransitionGuard>());
      }
    }
  }
}