using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FluentAssertions;
using NUnit.Framework;
using ProjectionSystem.ModelsForTest;

namespace ProjectionSystem.States {
  [TestFixture]
  public class UninitialisedStateTests {
    UninitialisedState<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _sut = new UninitialisedState<Department>();
    }

    [TestFixture]
    public class Construction : UninitialisedStateTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class Id : UninitialisedStateTests {
      [Test]
      public void ReturnsCorrectId() {
        Assert.That(_sut.Id, Is.EqualTo(StateId.Uninitialised));
      }
    }

    [TestFixture]
    public class IsTransitionAllowed : UninitialisedStateTests {
      [Test]
      public void CanOnlyBeTheInitialState() {
        Assert.That(_sut.IsTransitionAllowed(null), Is.True);
      }

      [TestCase(StateId.Uninitialised)]
      [TestCase(StateId.Creating)]
      [TestCase(StateId.Updating)]
      [TestCase(StateId.Expired)]
      [TestCase(StateId.Valid)]
      public void PreviousStateIsNotAllowed(StateId id) {
        Assert.That(_sut.IsTransitionAllowed(id), Is.False);
      }
    }

    [TestFixture]
    public class BeforeEnter : UninitialisedStateTests {
      [Test]
      public void DoesNothing() {
        Assert.DoesNotThrowAsync(() => _sut.BeforeEnter());
      }
    }

    [TestFixture]
    public class AfterEnter : UninitialisedStateTests {
      [Test]
      public void DoesNothing() {
        Assert.DoesNotThrowAsync(() => _sut.AfterEnter());
      }
    }

    [TestFixture]
    public class GetProjection : UninitialisedStateTests {
      [Test]
      public async Task ReturnsNull() {
        var actual = await _sut.GetProjection();
        Assert.That(actual, Is.Null);
      }
    }
  }
}