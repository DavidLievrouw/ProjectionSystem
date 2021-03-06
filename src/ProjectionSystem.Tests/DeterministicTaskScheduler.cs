﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjectionSystem {
  /// <summary>
  /// A TaskScheduler to be used for unit testing. 
  /// The class allows to execute new scheduled tasks on the same thread as a unit test.
  /// </summary>
  public class DeterministicTaskScheduler : TaskScheduler {
    public List<Task> ScheduledTasks { get; } = new List<Task>();
    public List<Task> FinishedTasks { get; } = new List<Task>();

    /// <summary>
    /// Runs only the currently scheduled tasks.
    /// </summary>
    public void RunPendingTasks() {
      foreach (var task in ScheduledTasks.ToArray()) {
        TryExecuteTask(task);

        // Propagate exceptions
        try {
          task.Wait();
        } catch (AggregateException aggregateException) {
          throw aggregateException.InnerException;
        } finally {
          ScheduledTasks.Remove(task);
          FinishedTasks.Add(task);
        }
      }
    }

    /// <summary>
    /// Runs all tasks until no more scheduled tasks are left.
    /// If a pending task schedules an additional task it will also be executed.
    /// </summary>
    public void RunTasksUntilIdle() {
      while (ScheduledTasks.Any()) {
        RunPendingTasks();
      }
    }

    #region TaskScheduler methods

    protected override void QueueTask(Task task) {
      ScheduledTasks.Add(task);
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued) {
      ScheduledTasks.Add(task);
      return false;
    }

    protected override IEnumerable<Task> GetScheduledTasks() {
      return ScheduledTasks;
    }

    public override int MaximumConcurrencyLevel => 1;

    #endregion
  }
}