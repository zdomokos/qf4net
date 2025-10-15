# QF4NET - Quantum Framework for .NET

[![CI/CD](https://github.com/zdomokos/qf4net/actions/workflows/ci.yml/badge.svg)](https://github.com/zdomokos/qf4net/actions/workflows/ci.yml)
[![CodeQL](https://github.com/zdomokos/qf4net/actions/workflows/codeql.yml/badge.svg)](https://github.com/zdomokos/qf4net/actions/workflows/codeql.yml)
[![NuGet Version](https://img.shields.io/nuget/v/qf4net.svg)](https://www.nuget.org/packages/qf4net/)
[![NuGet Downloads](https://img.shields.io/nuget/dt/qf4net.svg)](https://www.nuget.org/packages/qf4net/)
[![License](https://img.shields.io/github/license/zdomokos/qf4net.svg)](LICENSE)

A modern .NET implementation of Miro Samek's Quantum Framework for building event-driven applications with hierarchical state machines. Adapted for .NET 8.0 with C# 12 support.

## Features

- **Hierarchical State Machines** - Full UML statechart implementation with nested states
- **Event-Driven Architecture** - Publish-subscribe system with priority-based delivery
- **Active Objects** - Concurrent state machines with independent event queues
- **Thread-Safe** - Synchronized operations for multi-threaded applications
- **Timers** - Built-in time-based event support
- **Tracing** - Configurable state event debugging (None/UserSignals/All)
- **Cancellation** - CancellationToken support for event pumps
- **Dependency Injection** - Event broker singleton injection

## Installation

```bash
dotnet add package qf4net
```

## Core Classes

| Class | Description | Use When |
|-------|-------------|----------|
| `QFsm` | Flat state machine | Simple 3-5 state logic without nesting |
| `QHsm` | Hierarchical state machine | Complex nested states and inheritance |
| `QActive` | Active object with event pump | Multi-threaded concurrent state machines |
| `QHsmWithTransitionChains` | Optimized HSM | Performance-critical applications |

## Quick Start

### Basic State Machine Example

```csharp
using qf4net;

// 1. Define custom signals
public class MySignals
{
    public static readonly QSignal SigA = new();
    public static readonly QSignal SigB = new();
}

// 2. Create state machine
public class MyStateMachine : QHsm
{
    private QState _state1;
    private QState _state2;

    public MyStateMachine()
    {
        _state1 = State1;
        _state2 = State2;
    }

    protected override void InitializeStateMachine()
    {
        InitializeState(_state1);
    }

    private QState State1(IQEvent qEvent)
    {
        if (qEvent.IsSignal(QSignals.Entry))
        {
            Console.WriteLine("Entered State1");
            return null;
        }
        if (qEvent.IsSignal(MySignals.SigA))
        {
            TransitionTo(_state2);
            return null;
        }
        return TopState;
    }

    private QState State2(IQEvent qEvent)
    {
        if (qEvent.IsSignal(QSignals.Entry))
        {
            Console.WriteLine("Entered State2");
            return null;
        }
        if (qEvent.IsSignal(MySignals.SigB))
        {
            TransitionTo(_state1);
            return null;
        }
        return TopState;
    }
}

// 3. Use the state machine
var sm = new MyStateMachine();
sm.Init();
sm.Dispatch(new QEvent(MySignals.SigA));  // State1 -> State2
sm.Dispatch(new QEvent(MySignals.SigB));  // State2 -> State1
```

## Configuration

```csharp
var config = new StatemachineConfig
{
    TraceLevel = TraceLevel.All,       // None, UserSignals, All
    SendStateJobAfterEntry = false
};

var sm = new MyStateMachine(config);

// Custom tracing
protected override void StateEventTrace(QState state, QSignal signal)
{
    Console.WriteLine($"[TRACE] {signal} in {state.Method.Name}");
}
```

## Active Objects (Concurrent State Machines)

```csharp
public class MyActive : QActive
{
    public MyActive() : base(QEventBrokerSingleton.Instance) { }

    protected override void InitializeStateMachine()
    {
        QEventBrokerSingleton.Instance.Subscribe(this, MySignals.MySig);
        InitializeState(InitialState);
    }
}

// Run with cancellation
var cts = new CancellationTokenSource();
var active = new MyActive();
active.Init();
Task.Run(() => active.DoEventLoop(cts.Token));

// Publish events
QEventBrokerSingleton.Instance.Publish(new QEvent(MySignals.MySig));

// Cancel when done
cts.Cancel();
```

## Examples

Run examples from `tests/Examples/`:
- **CalculatorHSM** - Calculator with hierarchical states
- **DiningPhilosophers** - Classic concurrency problem with active objects
- **OrthogonalComponent** - Parallel state machines
- **QHsmTest** - Basic HSM patterns

```bash
cd tests/Examples/CalculatorHSM && dotnet run
```

## Testing

```bash
dotnet test
```

## Version History

- **25.9.27.1** - TraceLevel enum, event pump cancellation, timer improvements, .NET 8.0/C# 12
- **24.7.9** - Bug fixes and performance improvements
- **24.5.8** - StateTrace functionality

## References

- **Original Work**: Miro Samek, Ph.D. - "Practical Statecharts in C/C++; Quantum Programming for Embedded Systems"
- **Quantum Leaps**: http://www.quantum-leaps.com/
- **License**: BSD-3-Clause

---

*Hierarchical state machines for .NET - building robust event-driven applications with clean, maintainable code.*
