using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DavidLievrouw.Utils;
using NUnit.Framework;

namespace ProjectionSystem.Samples.Departments {
  [TestFixture]
  public class InitialisationTests {
    DepartmentsProjectionSystem _sut;
    RealSystemClock _systemClock;
    TimeSpan _expiration;

    [SetUp]
    public void SetUp() {
      _systemClock = new RealSystemClock();
      _expiration = TimeSpan.FromSeconds(2);
      _sut = new DepartmentsProjectionSystem(
        new DepartmentsProjectionDataService(_systemClock),
        _expiration);
    }

    [Test]
    public void WhenInitialising_ThreadsWaitForInitialisation() {
      Console.WriteLine($"Starting test at {_systemClock.UtcNow}");
      var threads = new List<Thread>();
      for (var i = 0; i < 3; i++) threads.Add(new Thread(() => {
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
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(1)));
      var threads = new List<Thread>();
      for (var i = 0; i < 3; i++) threads.Add(new Thread(() => {
        var departments = _sut.GetProjectedDepartments();
        Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} returns departments of {departments.Max(dep => dep.ProjectionTime)}.");
      }));
      threads.ForEach(t => t.Start());
      threads.ForEach(t => t.Join());
    }

    [Test]
    public void StressTestThreadSafety() {}
  }
}