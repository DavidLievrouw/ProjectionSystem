using System;
using System.Runtime.Serialization;

namespace ProjectionSystem.States.Transitions {
  public class InvalidStateTransitionException : InvalidOperationException {
    public InvalidStateTransitionException() {}
    public InvalidStateTransitionException(string message) : base(message) {}
    public InvalidStateTransitionException(string message, Exception innerException) : base(message, innerException) {}
    protected InvalidStateTransitionException(SerializationInfo info, StreamingContext context) : base(info, context) {}

    public InvalidStateTransitionException(IState oldState, IState newState) {
      OldState = oldState;
      NewState = newState;
    }

    public InvalidStateTransitionException(IState oldState, IState newState, string message) : base(message) {
      OldState = oldState;
      NewState = newState;
    }

    public InvalidStateTransitionException(IState oldState, IState newState, string message, Exception innerException) : base(message, innerException) {
      OldState = oldState;
      NewState = newState;
    }

    public IState OldState { get; }
    public IState NewState { get; }
  }
}