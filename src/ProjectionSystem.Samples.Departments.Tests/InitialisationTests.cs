using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DavidLievrouw.Utils;
using NUnit.Framework;

namespace ProjectionSystem.Samples.Departments {
  [TestFixture]
  public class InitialisationTests {
    DepartmentsProjectionSystem _sut;
    DepartmentsProjectionDataService _projectionDataService;
    RealSystemClock _systemClock;
    TimeSpan _expiration;

    [SetUp]
    public void SetUp() {
      _systemClock = new RealSystemClock();
      _expiration = TimeSpan.FromSeconds(10);
      _projectionDataService = new DepartmentsProjectionDataService(_systemClock);
      _sut = new DepartmentsProjectionSystem(_expiration, _projectionDataService);
    }

    [Test]
    public void WhenInitialising_ThreadsWaitForInitialisation() {
      Console.WriteLine($"Starting test at {_systemClock.UtcNow}");
      var threads = new List<Thread>();
      for (var i = 0; i < 3; i++)
        threads.Add(new Thread(() => {
          try {
            var departments = _sut.GetProjectedDepartments();
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} returns departments of {departments.Max(dep => dep.ProjectionTime)}.");
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        }));
      threads.ForEach(t => t.Start());
      threads.ForEach(t => t.Join());
    }

    [Test]
    public void WhenExpired_OneThreadRefreshesData_AndOtherThreadsReturnOldData() {
      var originalDepartments = _sut.GetProjectedDepartments();
      Console.WriteLine($"Initialised departments of {originalDepartments.Max(dep => dep.ProjectionTime)}.");
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(1)));
      var threads = new List<Thread>();
      for (var i = 0; i < 3; i++)
        threads.Add(new Thread(() => {
          try {
            var departments = _sut.GetProjectedDepartments();
            Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} returns departments of {departments.Max(dep => dep.ProjectionTime)}.");
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        }));
      threads.ForEach(t => t.Start());
      threads.ForEach(t => t.Join());
    }

    [Test]
    public void StressTestThreadSafety() {}
  }
}