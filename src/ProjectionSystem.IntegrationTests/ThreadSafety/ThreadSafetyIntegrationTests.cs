using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using ProjectionSystem.IntegrationTests.Items;
using ProjectionSystem.States;

namespace ProjectionSystem.IntegrationTests.ThreadSafety {
  [TestFixture(Category = "Integration")]
  public class ThreadSafetyIntegrationTests {
    TimeSpan _expiration;
    TimeSpan _updateDuration;
    Model.ProjectionDataServiceForTest _projectionDataService;
    IProjectionSystem<Department> _sut;

    [OneTimeSetUp]
    public void OneTimeSetUp() {
      SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
    }

    [SetUp]
    public void SetUp() {
      _expiration = TimeSpan.FromSeconds(0.5);
      _updateDuration = TimeSpan.FromSeconds(0.25);
      _projectionDataService = new Model.ProjectionDataServiceForTest(_updateDuration);
      _sut = Model.Create(_expiration, _projectionDataService);
    }

    [Test]
    public void WhenProjectionExpiresBeforeCreateIsFinished_ExpiredDataIsStillAccessible() {
      // Reconfigure from setup
      _expiration = TimeSpan.FromSeconds(0.25);
      _updateDuration = TimeSpan.FromSeconds(0.5);
      _sut = Model.Create(_expiration, _projectionDataService);
      
      var query1 = _sut.GetLatestProjectionTime();
      Assert.That(query1, Is.Not.Null);
      Assert.That(_sut.State, Is.InstanceOf<CurrentState<Department>>());

      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25)));
      Assert.That(_sut.State, Is.InstanceOf<ExpiredState<Department>>());
    }

    [Test]
    public void WhenProjectionExpiresBeforeUpdateIsFinished_ExpiredDataIsStillAccessible() {
      // Reconfigure from setup
      _expiration = TimeSpan.FromSeconds(0.25);
      _updateDuration = TimeSpan.FromSeconds(0.5);
      _sut = Model.Create(_expiration, _projectionDataService);

      _sut.GetLatestProjectionTime();
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25))); // Expire

      var query1 = _sut.GetLatestProjectionTime(); // Trigger update
      Assert.That(query1, Is.Not.Null);
      Assert.That(_sut.State, Is.InstanceOf<UpdatingState<Department>>());

      Thread.Sleep(_updateDuration); // Wait for update to finish
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25))); // Expire
      Assert.That(_sut.State, Is.InstanceOf<ExpiredState<Department>>());
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
      Thread.Sleep(_expiration.Add(_updateDuration).Add(_updateDuration)); // Wait until refresh is certainly finished
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

      Thread.Sleep(_expiration.Add(_updateDuration).Add(_updateDuration)); // Wait until last refresh is certainly finished

      Assert.That(_projectionDataService.RefreshCount, Is.EqualTo(3));
    }
  }
}