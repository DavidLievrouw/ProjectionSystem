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
  public class ExpiredStateTests {
    IProjectionSystem<Department> _projectionSystem;
    ExpiredState<Department> _sut;

    [SetUp]
    public virtual void SetUp() {
      _projectionSystem = _projectionSystem.Fake();
      _sut = new ExpiredState<Department>(_projectionSystem);
    }

    [TestFixture]
    public class Construction : ExpiredStateTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class Id : ExpiredStateTests {
      [Test]
      public void ReturnsCorrectId() {
        Assert.That(_sut.Id, Is.EqualTo(StateId.Expired));
      }
    }

    [TestFixture]
    public class IsTransitionAllowed : ExpiredStateTests {
      [Test]
      public void PreviousStateIsRequired() {
        Assert.That(_sut.IsTransitionAllowed(null), Is.False);
      }

      [TestCase(StateId.Uninitialised)]
      [TestCase(StateId.Creating)]
      [TestCase(StateId.Updating)]
      [TestCase(StateId.Expired)]
      public void NotAllowedToTransitionFromState(StateId id) {
        Assert.That(_sut.IsTransitionAllowed(id), Is.False);
      }

      [TestCase(StateId.Valid)]
      public void AllowedToTransitionFromState(StateId id) {
        Assert.That(_sut.IsTransitionAllowed(id), Is.True);
      }
    }

    [TestFixture]
    public class BeforeEnter : ExpiredStateTests {
      IState<Department> _validState;
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _validState = _validState.Fake();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task KeepsTrackOfPreviousProjection() {
        A.CallTo(() => _projectionSystem.State)
          .Returns(_validState);
        A.CallTo(() => _validState.GetProjection())
          .Returns(_expectedProjection);

        await _sut.BeforeEnter();

        var currentProjection = await _sut.GetProjection();
        Assert.That(currentProjection, Is.EqualTo(_expectedProjection));
      }
    }

    [TestFixture]
    public class AfterEnter : ExpiredStateTests {
      [Test]
      public void DoesNothing() {
        Assert.DoesNotThrowAsync(() => _sut.AfterEnter());
      }
    }

    [TestFixture]
    public class GetProjection : ExpiredStateTests {
      IState<Department> _validState;
      List<Department> _expectedProjection;

      [SetUp]
      public override void SetUp() {
        base.SetUp();
        _validState = _validState.Fake();
        _expectedProjection = new List<Department>(new[] { new Department { Id = 1 } });
      }

      [Test]
      public async Task ReturnsProjectionThatWasFetchedBefore() {
        // Make sure something is in the field
        A.CallTo(() => _projectionSystem.State)
          .Returns(_validState);
        A.CallTo(() => _validState.GetProjection())
          .Returns(_expectedProjection);
        await _sut.BeforeEnter();

        // Check if the method returns what is in the field
        var currentProjection = await _sut.GetProjection();
        Assert.That(currentProjection, Is.EqualTo(_expectedProjection));
      }
    }
  }
}