# QF4NET - Quantum Framework for .NET

A C# port of Miro Samek's Quantum Framework, featuring an excellent implementation of hierarchical state machines for event-driven programming.

## Overview

QF4NET is a modern .NET implementation of the Quantum Framework originally developed by Miro Samek. This framework provides a robust foundation for building event-driven applications using hierarchical state machines (HSMs). The implementation has been adapted for desktop systems while maintaining the core concepts and benefits of the original embedded systems framework.

## Features

- **Hierarchical State Machines**: Full implementation of UML statecharts with hierarchical states
- **Event-Driven Architecture**: Publish-subscribe event system with priority-based delivery
- **Thread-Safe Operations**: Synchronized event handling suitable for multi-threaded applications
- **Active Objects**: Support for concurrent active objects with independent event queues
- **Queue Management**: Flexible event queue implementations with FIFO and LIFO support
- **Timer Support**: Built-in timer functionality for time-based events
- **.NET 8.0 Compatible**: Modern C# implementation targeting .NET 8.0

## Installation

### NuGet Package
```bash
dotnet add package qf4net
```

### Building from Source
```bash
git clone https://github.com/zdomokos/qf4net.git
cd qf4net
dotnet build
```

## Quick Start

### Define Signals

```csharp
using qf4net;

// Define your custom signals
public class MySignals : QSignals
{
    public static readonly Signal SigA = new(nameof(SigA));
    public static readonly Signal SigB = new(nameof(SigB));
}
```

### Basic State Machine

```csharp
public class MyStateMachine : QHsm
{
    private QState m_initial;
    private QState m_state1;
    private QState m_state2;

    public MyStateMachine()
    {
        m_initial = Initial;
        m_state1 = State1;
        m_state2 = State2;
    }

    protected override void InitializeStateMachine()
    {
        Console.WriteLine("Initializing state machine");
        InitializeState(m_initial);
    }

    private QState Initial(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.WriteLine("Initial-ENTRY");
            return null;
        }
        if (qEvent.QSignal == QSignals.Init)
        {
            Console.WriteLine("Initial-INIT");
            InitializeState(m_state1);
            return null;
        }
        return TopState;
    }

    private QState State1(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.WriteLine("State1-ENTRY");
            return null;
        }
        if (qEvent.QSignal == QSignals.Exit)
        {
            Console.WriteLine("State1-EXIT");
            return null;
        }
        if (qEvent.QSignal == MySignals.SigA)
        {
            Console.WriteLine("State1-SigA: transitioning to State2");
            TransitionTo(m_state2);
            return null;
        }
        return m_initial;
    }

    private QState State2(IQEvent qEvent)
    {
        if (qEvent.QSignal == QSignals.Entry)
        {
            Console.WriteLine("State2-ENTRY");
            return null;
        }
        if (qEvent.QSignal == MySignals.SigB)
        {
            Console.WriteLine("State2-SigB: transitioning to State1");
            TransitionTo(m_state1);
            return null;
        }
        return m_initial;
    }
}
```

### Running the State Machine

```csharp
// Create and initialize state machine
var stateMachine = new MyStateMachine();
stateMachine.Init(); // Take the initial transition

// Dispatch events to the state machine
stateMachine.Dispatch(new QEvent(MySignals.SigA));
stateMachine.Dispatch(new QEvent(MySignals.SigB));
```

## Examples

The repository includes several comprehensive examples:

- **Calculator HSM**: A calculator application demonstrating hierarchical state machines
- **Dining Philosophers**: Classic concurrency problem solved with active objects
- **Alarm Clock**: Orthogonal components and time-based events
- **Reminder Pattern**: Event reminder and scheduling patterns

### Running Examples

```bash
cd Examples/CalculatorHSM
dotnet run
```

## Architecture

### Core Components

- **QF**: Singleton framework manager handling event routing and subscriptions
- **QHsm**: Base class for hierarchical state machines
- **QActive**: Base class for active objects with event queues
- **QEvent**: Base event class for all framework events
- **QTimer**: Timer implementation for time-based events

### Key Concepts

1. **Hierarchical States**: States can be nested, allowing for state inheritance and code reuse
2. **Event-Driven**: All communication happens through events, promoting loose coupling
3. **Active Objects**: Concurrent objects that process events in their own thread context
4. **Publish-Subscribe**: Flexible event routing with priority-based delivery

## API Reference

### QF (Quantum Framework)

```csharp
// Initialize framework
QF.Instance.Initialize(maxSignal);

// Subscribe to events
QF.Instance.Subscribe(activeObject, signal);

// Publish events
QF.Instance.Publish(qEvent);
```

### QHsm (Hierarchical State Machine)

```csharp
// Define state methods
private QState MyState(IQEvent qEvent)
{
    if (qEvent.QSignal == QSignals.Entry)
    {
        // Entry action
        Console.WriteLine("MyState-ENTRY");
        return null;
    }
    if (qEvent.QSignal == QSignals.Exit)
    {
        // Exit action
        Console.WriteLine("MyState-EXIT");
        return null;
    }
    if (qEvent.QSignal == MySignals.MySig)
    {
        // Transition to target state
        TransitionTo(TargetState);
        return null;
    }
    // Return parent state for hierarchical structure
    return ParentState;
}

// Initialize state machine
protected override void InitializeStateMachine()
{
    InitializeState(InitialState);
}

// Dispatch events
stateMachine.Dispatch(new QEvent(MySignals.MySig));
```

## Testing

Run the unit tests:

```bash
dotnet test
```

## Version History

- **25.9.23**: Latest stable release with .NET 8.0 support
- **24.7.9**: Bug fixes and performance improvements
- **24.5.8**: StateTrace level functionality added
- Previous versions available in commit history

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Submit a pull request

## License

This project is licensed under a BSD-style license. See the source code headers for full license text.

## References

- **Original Work**: "Practical Statecharts in C/C++; Quantum Programming for Embedded Systems" by Miro Samek, Ph.D.
- **Quantum Leaps**: http://www.quantum-leaps.com/
- **Original SourceForge Project**: https://sourceforge.net/projects/qf4net/

## Support

- **Issues**: Report bugs and feature requests on GitHub
- **Documentation**: See inline code documentation and examples
- **Community**: Join discussions in the GitHub repository

---

*QF4NET brings the power of hierarchical state machines to the .NET ecosystem, enabling robust event-driven applications with clean, maintainable code.*