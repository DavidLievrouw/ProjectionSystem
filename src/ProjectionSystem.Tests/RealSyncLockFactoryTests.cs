using System.Threading;
using System.Threading.Tasks;
using DavidLievrouw.Utils.ForTesting.FluentAssertions;
using FluentAssertions;
using NUnit.Framework;

namespace ProjectionSystem {
  [TestFixture]
  public class RealSyncLockFactoryTests {
    SemaphoreSlim _semaphore;
    RealSyncLockFactory _sut;

    [SetUp]
    public void SetUp() {
      _sut = new RealSyncLockFactory(_semaphore);
    }

    [OneTimeSetUp]
    public void OneTimeSetUp() {
      _semaphore = new SemaphoreSlim(1);
    }

    [OneTimeTearDown]
    public void OneTimeTearDown() {
      _semaphore.Dispose();
    }

    [TestFixture]
    public class Construction : RealSyncLockFactoryTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        _sut.Should().HaveExactlyOneConstructorWithoutOptionalParameters();
      }
    }

    [TestFixture]
    public class Create : RealSyncLockFactoryTests {
      [Test]
      public async Task CreatesDisposableLock() {
        using (var actual = await _sut.Create()) {
          Assert.That(actual, Is.InstanceOf<RealSyncLock>());
        }
      }
    }
  }
}