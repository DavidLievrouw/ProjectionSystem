using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ProjectionSystem.IntegrationTests.Items;
using ProjectionSystem.States;
using ProjectionSystem.States.Transitions;

namespace ProjectionSystem.IntegrationTests.ThreadSafety {
  [TestFixture(Category = "Integration")]
  public class ThreadSafetyAndFlowIntegrationTests {
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
    public async Task WhenProjectionExpiresBeforeCreateIsFinished_ExpiredDataIsStillAccessible() {
      // Reconfigure from setup
      _expiration = TimeSpan.FromSeconds(0.25);
      _updateDuration = TimeSpan.FromSeconds(0.5);
      _sut = Model.Create(_expiration, _projectionDataService);
      
      var query1 = await _sut.GetLatestProjectionTime();
      Assert.That(query1, Is.Not.Null);
      Assert.That(_sut.State, Is.InstanceOf<CurrentState<Department>>());

      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25)));
      Assert.That(_sut.State, Is.InstanceOf<ExpiredState<Department>>());
    }

    [Test]
    public async Task WhenProjectionExpiresBeforeUpdateIsFinished_ExpiredDataIsStillAccessible() {
      // Reconfigure from setup
      _expiration = TimeSpan.FromSeconds(0.25);
      _updateDuration = TimeSpan.FromSeconds(0.5);
      _sut = Model.Create(_expiration, _projectionDataService);

      await _sut.GetLatestProjectionTime();
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25))); // Expire

      var query1 = await _sut.GetLatestProjectionTime(); // Trigger update
      Assert.That(query1, Is.Not.Null);
      Assert.That(_sut.State, Is.InstanceOf<UpdatingState<Department>>());

      Thread.Sleep(_updateDuration); // Wait for update to finish
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25))); // Expire
      Assert.That(_sut.State, Is.InstanceOf<ExpiredState<Department>>());
    }

    [Test]
    public async Task WhenInitialising_Refreshes_ThenReturnsCurrentItems() {
      var query1 = await _sut.GetLatestProjectionTime();
      var query2 = await _sut.GetLatestProjectionTime();
      Assert.That(query1, Is.EqualTo(query2), "While still in current mode, the projection system should return the cached projection.");
    }

    [Test]
    public async Task WhenInitialised_RefreshesWhenExpirationPasses() {
      var query1 = await _sut.GetLatestProjectionTime();
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25)));
      var query2 = await _sut.GetLatestProjectionTime();
      Assert.That(query1, Is.EqualTo(query2), "While still in updating mode, the projection system should return the expired projection.");
    }

    [Test]
    public async Task WhenInitialised_RefreshesWhenExpirationPasses_AndReturnsNewProjectionWhenRefreshed() {
      var query1 = await _sut.GetLatestProjectionTime();
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25))); // Expire
      await _sut.GetLatestProjectionTime(); // Trigger update
      Thread.Sleep(_expiration.Add(_updateDuration).Add(_updateDuration)); // Wait until update is certainly finished
      var query3 = await _sut.GetLatestProjectionTime();
      Assert.That(query3, Is.GreaterThan(query1), "After updating, the projection system should return the new projection.");
    }

    [Test]
    public async Task WhenInvalidatingDuringAnUpdate_DoesSomething() {
      await _sut.GetLatestProjectionTime();
      Thread.Sleep(_expiration.Add(TimeSpan.FromSeconds(0.25))); // Expire
      await _sut.GetLatestProjectionTime(); // Trigger update
      Thread.Sleep(TimeSpan.FromMilliseconds(_updateDuration.TotalMilliseconds / 2)); // Updating now
      Assert.ThrowsAsync<InvalidStateTransitionException>(() => _sut.InvalidateProjection()); // Mark as Expired during Updating state
    }

    [Test]
    public void WhenInitialising_ThreadsWaitForInitialisation_ThenReturnSameData() {
      DateTimeOffset? previous = null;
      var threads = new List<Thread>();
      for (var i = 0; i < 3; i++)
        threads.Add(new Thread(async () => {
          try {
            var projectionTime = await _sut.GetLatestProjectionTime();
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
    public async Task StressTestThreadSafety() {
      await _sut.GetLatestProjectionTime(); // Make sure projection is current
      var tasks = new List<Task> {
        new Task(async () => {
          try {
            var j = 0;
            while (j < 50) {
              await Task.Delay(25);
              await _sut.GetLatestProjectionTime();
              Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss,fff")} T1 > Done iteration {j+1}.");
              j++;
            }
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        }, TaskCreationOptions.LongRunning),
        new Task(async () => {
          try {
            var j = 0;
            while (j < 25) {
              await Task.Delay(50);
              await _sut.GetLatestProjectionTime();
              Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss,fff")} T2 > Done iteration {j+1}.");
              j++;
            }
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        }, TaskCreationOptions.LongRunning),
        new Task(async () => {
          try {
            var j = 0;
            while (j < 11) {
              await Task.Delay(100);
              await _sut.GetLatestProjectionTime();
              Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss,fff")} T3 > Done iteration {j+1}.");
              j++;
            }
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        }, TaskCreationOptions.LongRunning),
        new Task(async () => {
          try {
            var j = 0;
            while (j < 8) {
              await Task.Delay(150);
              await _sut.GetLatestProjectionTime();
              Console.WriteLine($"{DateTime.Now.ToString("HH:mm:ss,fff")} T4 > Done iteration {j+1}.");
              j++;
            }
          } catch (Exception ex) {
            Assert.Fail(ex.ToString());
          }
        }, TaskCreationOptions.LongRunning)
      };
      tasks.ForEach(t => t.Start());

      // Test should take about 3 seconds 
      Thread.Sleep(3000);

      Assert.That(_projectionDataService.RefreshCount, Is.EqualTo(3));
    }
  }
}