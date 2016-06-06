using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using DavidLievrouw.Utils;
using NUnit.Framework;
using ProjectionSystem.Samples.Departments.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.Samples.Departments {
  [TestFixture(Category = "Integration")]
  public class IntegrationTests {
    TimeSpan _expiration;
    TimeSpan _refreshDuration;
    IProjectionSystem<Department> _sut;
    DepartmentsProjectionDataService _projectionDataService;

    [OneTimeSetUp]
    public void OneTimeSetUp() {
      SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
    }

    [SetUp]
    public void SetUp() {
      var stateSyncLockFactory = new RealSyncLockFactory(new object());
      var systemClock = new RealSystemClock();
      var traceLogger = new ConsoleTraceLogger(systemClock);
      var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
      var createProjectionLockFactory = new RealSyncLockFactory(new object());
      var updateProjectionLockFactory = new RealSyncLockFactory(new object());
      var transitionGuardFactory = new StateTransitionGuardFactory();
      
      _expiration = TimeSpan.FromSeconds(0.5);
      _refreshDuration = TimeSpan.FromSeconds(0.25);
      _projectionDataService = new DepartmentsProjectionDataService(_refreshDuration, systemClock, traceLogger);
      _sut = new DepartmentsProjectionSystemFactory(stateSyncLockFactory, traceLogger).Create(
        new UninitialisedState<Department>(transitionGuardFactory),
        new CurrentState<Department>(transitionGuardFactory, _expiration, taskScheduler),
        new ExpiredState<Department>(transitionGuardFactory),
        new UpdatingState<Department>(transitionGuardFactory, _projectionDataService, updateProjectionLockFactory, taskScheduler),
        new CreatingState<Department>(transitionGuardFactory, _projectionDataService, createProjectionLockFactory));
    }

    [Test]
    public void WhenProjectionExpiresBeforeCreateIsFinished_ExpiredDataIsStillAccessible() {
      Assert.Fail("Implement!");
    }

    [Test]
    public void WhenProjectionExpiresBeforeUpdateIsFinished_ExpiredDataIsStillAccessible() {
      Assert.Fail("Implement!");
    }

    [Test]
    public void WhenInitialising_Refreshes_ThenReturnsCurrentItems() {
      var query1 = _sut.GetLatestProjectionTime();
      var query2 = _sut.GetLatestProjectionTime();
      Assert.That(query1, Is.EqualTo(query2), "While still in current mode, the projection system should return the cached projection.");
    }

    [Test]
    public void WhenInitialised_RefreshesWhenExpirationPasses() {
      var query1 = _sut.GetLatestProjectionTime();
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25)));
      var query2 = _sut.GetLatestProjectionTime();
      Assert.That(query1, Is.EqualTo(query2), "While still in updating mode, the projection system should return the expired projection.");
    }

    [Test]
    public void WhenInitialised_RefreshesWhenExpirationPasses_AndReturnsNewProjectionWhenRefreshed() {
      var query1 = _sut.GetLatestProjectionTime();
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25))); // Expire
      _sut.GetLatestProjectionTime(); // Trigger update
      Thread.Sleep(_expiration.Add(_refreshDuration).Add(_refreshDuration)); // Wait until refresh is certainly finished
      var query3 = _sut.GetLatestProjectionTime();
      Assert.That(query3, Is.GreaterThan(query1), "After updating, the projection system should return the new projection.");
    }

    [Test]
    public void WhenInitialising_ThreadsWaitForInitialisation_ThenReturnSameData() {
      DateTimeOffset? previous = null;
      var threads = new List<Thread>();
      for (var i = 0; i < 3; i++)
        threads.Add(new Thread(() => {
          try {
            var projectionTime = _sut.GetLatestProjectionTime();
            if (previous.HasValue) Assert.That(projectionTime, Is.EqualTo(previous));
            previous = projectionTime;
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        }));
      threads.ForEach(t => t.Start());
      threads.ForEach(t => t.Join());
    }

    [Test]
    public void StressTestThreadSafety() {
      _sut.GetLatestProjectionTime(); // Make sure projection is current
      var watch = Stopwatch.StartNew();
      var threads = new List<Thread> {
        new Thread(() => {
          try {
            var j = 0;
            while (j < 25) {
              Thread.Sleep(50);
              _sut.GetLatestProjectionTime();
              j++;
            }
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        }),
        new Thread(() => {
          try {
            var j = 0;
            while (j < 11) {
              Thread.Sleep(100);
              _sut.GetLatestProjectionTime();
              j++;
            }
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        }),
        new Thread(() => {
          try {
            var j = 0;
            while (j < 8) {
              Thread.Sleep(150);
              _sut.GetLatestProjectionTime();
              j++;
            }
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        })
      };
      threads.ForEach(t => t.Start());
      threads.ForEach(t => t.Join());
      watch.Stop();

      Thread.Sleep(_expiration.Add(_refreshDuration).Add(_refreshDuration)); // Wait until last refresh is certainly finished

      Assert.That(_projectionDataService.RefreshCount, Is.EqualTo(3));
    }
  }
}