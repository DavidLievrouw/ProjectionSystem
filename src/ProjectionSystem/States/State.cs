using System;
using System.Collections.Generic;

namespace ProjectionSystem.States {
  public abstract class State : IState {
    public abstract void Enter(IState previousState);
    public abstract StateId Id { get; }
  }

  public abstract class State<TItem> : State, IState<TItem>
    where TItem : IProjectedItem {
    public abstract void Enter(IState<TItem> previousState);
    public abstract IEnumerable<TItem> GetProjectedData();

    void IState.Enter(IState previousState) {
      Enter(previousState);
    }

    public override void Enter(IState previousState) {
      var typedProjectionSystemState = previousState as IState<TItem>;
      if (typedProjectionSystemState == null) throw new ArgumentException($"The previous state of {previousState.GetType().Name} cannot be used in a projection system for {typeof(TItem).Name}.", nameof(previousState));
      Enter(typedProjectionSystemState);
    }
  }
}