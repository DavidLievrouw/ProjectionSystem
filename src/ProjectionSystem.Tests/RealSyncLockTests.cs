using System;
using System.Threading;
using NUnit.Framework;

namespace ProjectionSystem {
  [TestFixture]
  public class RealSyncLockTests {
    SemaphoreSlim _semaphore;
    RealSyncLock _sut;

    [SetUp]
    public void SetUp() {
      _sut = new RealSyncLock(_semaphore);
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
    public class Construction : RealSyncLockTests {
      [Test]
      public void HasExactlyOneConstructor_WithNoOptionalParameters() {
        Assert.Throws<ArgumentNullException>(() => new RealSyncLock(null));
      }
    }

    // Interface extracted code.
    // Useless to test the .NET SemaphoreSlim implementation.
  }
}
