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

    [SetUp]
    public void SetUp() {
      _systemClock = new RealSystemClock();
      _expiration = TimeSpan.FromSeconds(5);
      _projectionDataService = new DepartmentsProjectionDataService(_systemClock);
      _sut = new DepartmentsProjectionSystem(_expiration, _projectionDataService);
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
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(1)));
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
              Thread.Sleep(500);
              var departments = _sut.GetProjectedDepartments();
              Console.WriteLine($"{watch.ElapsedMilliseconds} > (500) Thread {Thread.CurrentThread.ManagedThreadId} returns departments of {departments.Max(dep => dep.ProjectionTime)}.");
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
              Thread.Sleep(1000);
              var departments = _sut.GetProjectedDepartments();
              Console.WriteLine($"{watch.ElapsedMilliseconds} > (1000) Thread {Thread.CurrentThread.ManagedThreadId} returns departments of {departments.Max(dep => dep.ProjectionTime)}.");
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
              Thread.Sleep(1500);
              var departments = _sut.GetProjectedDepartments();
              Console.WriteLine($"{watch.ElapsedMilliseconds} > (1500) Thread {Thread.CurrentThread.ManagedThreadId} returns departments of {departments.Max(dep => dep.ProjectionTime)}.");
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