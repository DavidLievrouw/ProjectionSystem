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
    ConsoleTraceLogger _traceLogger;

    [SetUp]
    public void SetUp() {
      _systemClock = new RealSystemClock();
      _expiration = TimeSpan.FromSeconds(0.5);
      _traceLogger = new ConsoleTraceLogger(_systemClock);
      _projectionDataService = new DepartmentsProjectionDataService(_systemClock, _traceLogger);
      _sut = new DepartmentsProjectionSystem(_expiration, _projectionDataService, _traceLogger);
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
      Assert.That(query2, Is.GreaterThan(query1), "After updating, the projection system should return the new projection.");
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
      _sut.GetProjectedDepartments();
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

      Assert.That(_projectionDataService.RefreshCount, Is.EqualTo(3));
    }
  }
}