using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using DavidLievrouw.Utils;
using NUnit.Framework;

namespace ProjectionSystem.Samples.Departments {
  [TestFixture(Category = "Integration")]
  public class IntegrationTests {
    DepartmentsProjectionSystem _sut;
    DepartmentsProjectionDataService _projectionDataService;
    RealSystemClock _systemClock;
    TimeSpan _expiration;
    TimeSpan _refreshDuration;
    ConsoleTraceLogger _traceLogger;

    [SetUp]
    public void SetUp() {
      _systemClock = new RealSystemClock();
      _expiration = TimeSpan.FromSeconds(0.5);
      _refreshDuration = TimeSpan.FromSeconds(0.25);
      _traceLogger = new ConsoleTraceLogger(_systemClock);
      _projectionDataService = new DepartmentsProjectionDataService(_refreshDuration, _systemClock, _traceLogger);
      _sut = new DepartmentsProjectionSystem(_expiration, _projectionDataService, _traceLogger);
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
      var query1 = _sut.GetProjectedDepartments().Max(dep => dep.ProjectionTime);
      var query2 = _sut.GetProjectedDepartments().Max(dep => dep.ProjectionTime);
      Assert.That(query1, Is.EqualTo(query2), "While still in current mode, the projection system should return the cached projection.");
    }

    [Test]
    public void WhenInitialised_RefreshesWhenExpirationPasses() {
      var query1 = _sut.GetProjectedDepartments().Max(dep => dep.ProjectionTime);
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25)));
      var query2 = _sut.GetProjectedDepartments().Max(dep => dep.ProjectionTime);
      Assert.That(query1, Is.EqualTo(query2), "While still in updating mode, the projection system should return the expired projection.");
    }

    [Test]
    public void WhenInitialised_RefreshesWhenExpirationPasses_AndReturnsNewProjectionWhenRefreshed() {
      var query1 = _sut.GetProjectedDepartments().Max(dep => dep.ProjectionTime);
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25))); // Expire
      _sut.GetProjectedDepartments().Max(dep => dep.ProjectionTime); // Trigger update
      Thread.Sleep(_expiration.Add(_refreshDuration).Add(_refreshDuration)); // Wait until refresh is certainly finished
      var query3 = _sut.GetProjectedDepartments().Max(dep => dep.ProjectionTime);
      Assert.That(query3, Is.GreaterThan(query1), "After updating, the projection system should return the new projection.");
    }

    [Test]
    public void WhenInitialising_ThreadsWaitForInitialisation_ThenReturnSameData() {
      DateTimeOffset? previous = null;
      var threads = new List<Thread>();
      for (var i = 0; i < 3; i++)
        threads.Add(new Thread(() => {
          try {
            var projectionTime = _sut.GetProjectedDepartments().Max(dep => dep.ProjectionTime);
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
      _sut.GetProjectedDepartments(); // Make sure projection is current
      var watch = Stopwatch.StartNew();
      var threads = new List<Thread> {
        new Thread(() => {
          try {
            var j = 0;
            while (j < 25) {
              Thread.Sleep(50);
              _sut.GetProjectedDepartments();
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
              _sut.GetProjectedDepartments();
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
              _sut.GetProjectedDepartments();
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