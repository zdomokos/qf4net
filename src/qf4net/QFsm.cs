// -----------------------------------------------------------------------------
//                            qf4net Library
//
// Port of Samek's Quantum Framework to C#. The implementation takes the liberty
// to depart from Miro Samek's code where the specifics of desktop systems
// (compared to embedded systems) seem to warrant a different approach.
// Please see accompanying documentation for details.
//
// Reference:
// Practical Statecharts in C/C++; Quantum Programming for Embedded Systems
// Author: Miro Samek, Ph.D.
// http://www.quantum-leaps.com/book.htm
//
// -----------------------------------------------------------------------------
//
// Copyright (C) 2003-2004, The qf4net Team
// All rights reserved
// Lead: Rainer Hessmer, Ph.D. (rainer@hessmer.org)
//
//
//   Redistribution and use in source and binary forms, with or without
//   modification, are permitted provided that the following conditions
//   are met:
//
//     - Redistributions of source code must retain the above copyright
//        notice, this list of conditions and the following disclaimer.
//
//     - Neither the name of the qf4net-Team, nor the names of its contributors
//        may be used to endorse or promote products derived from this
//        software without specific prior written permission.
//
//   THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//   "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//   LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS
//   FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL
//   THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
//   INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
//   (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//   SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
//   HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
//   STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
//   ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED
//   OF THE POSSIBILITY OF SUCH DAMAGE.
// -----------------------------------------------------------------------------

using System.Reflection;

namespace qf4net;

/// <summary>
/// Specifies the level of state event tracing for debugging and monitoring.
/// </summary>
public enum TraceLevel
{
    /// <summary>
    /// No tracing is performed.
    /// </summary>
    None = 0,

    /// <summary>
    /// Trace only user-defined signals (excludes Entry, Exit, Init, Empty lifecycle signals).
    /// </summary>
    UserSignals = 1,

    /// <summary>
    /// Trace all signals including lifecycle signals (Entry, Exit, Init, Empty).
    /// </summary>
    All = 2
}

public class StatemachineConfig
{
    /// <summary>
    /// Gets or sets whether to send a StateJob signal after Entry action.
    /// Default is false.
    /// </summary>
    public bool SendStateJobAfterEntry { get; set; } = false;

    /// <summary>
    /// Gets or sets the trace level for state event debugging.
    /// Default is None (no tracing).
    /// </summary>
    public TraceLevel TraceLevel { get; set; } = TraceLevel.None;

    /// <summary>
    /// Gets whether tracing is enabled (any level other than None).
    /// </summary>
    public bool IsTracingEnabled => TraceLevel != TraceLevel.None;
}

/// <summary>
/// The base class for all state machines (non-hierarchical)
/// </summary>
public class QFsm : IQHsm
{
    /// <summary>
    /// Constructor for the Quantum State Machine.
    /// </summary>
    protected QFsm(StatemachineConfig config = null)
    {
        Config      = config ?? new StatemachineConfig();
        StateMethod = TopState;
    }

    public StatemachineConfig Config { get; set; }

    /// <summary>
    /// Is called inside the function Init to give the deriving class a chance to
    /// initialize the state machine.
    /// Do not call base.InitializeStateMachine() from derived classes.
    /// </summary>
    protected virtual void InitializeStateMachine() { }

    /// <summary>
    /// Do not call this method directly, if using the event pump.
    /// Called by the event pump DoEventLoop method.
    /// Must only be called once by the client of the state machine to initialize the machine.
    /// Do not call base.Init() from derived classes.
    /// </summary>
    public virtual void Init()
    {
        InitializeStateMachine();

        if (StateMethod != null)
        {
            Trigger(StateMethod, QSignals.Entry);
            Trigger(StateMethod, QSignals.Init);

            if (Config.SendStateJobAfterEntry)
                Trigger(StateMethod, QSignals.StateJob);
        }
    }

    /// <summary>
    /// Determines whether the state machine is in the state specified by <see paramref="inquiredState"/>.
    /// Do not call base.InitializeStateMachine() from derived classes.
    /// </summary>
    /// <param name="inquiredState"></param>
    /// <returns></returns>
    public virtual bool IsInState(QState inquiredState)
    {
        return StateMethod == inquiredState;
    }

    public QState StateMethod       { get; protected set; }
    public QState SourceStateMethod { get; protected set; }

    /// <summary>
    /// Returns the name of the (deepest) state that the state machine is currently in.
    /// </summary>
    public virtual string CurrentStateName => StateMethod?.Method.Name ?? "Unknown";

    /// <summary>
    /// Dispatches the specified event to this state machine
    /// Do not call base.Dispatch() from the derived class.
    /// </summary>
    /// <param name="qEvent">The <see cref="IQEvent"/> to dispatch.</param>
    public virtual void Dispatch(IQEvent qEvent)
    {
        if (qEvent == null)
            return;

        lock (_sync)
        {
            try
            {
                var dispatchToState = StateMethod;

                while (!(dispatchToState == null || dispatchToState == TopState))
                {
                    StateEventTrace(dispatchToState, qEvent.Signal);
                    dispatchToState = dispatchToState.Invoke(qEvent);
                }
            }
            catch (TargetInvocationException tie)
            {
                var e                 = tie.InnerException;
                var exceptionMessages = "";
                while (e != null)
                {
                    exceptionMessages += $"{e.Message}{Environment.NewLine}";
                    e                 =  e.InnerException;
                }

                var message = $"The following unhandled exception was generated by {this}:\n";
                message += exceptionMessages;

                throw new Exception(message);
            }
        }
    }

    /// <summary>
    /// Represents the macro Q_INIT in Miro Samek's implementation
    /// </summary>
    public virtual void InitializeState(QState state)
    {
        StateMethod       = state;
        SourceStateMethod = StateMethod;
    }

    /// <summary>
    /// Performs a simple transition to the target state.
    /// </summary>
    /// <param name="targetState">The <see cref="QState"/> to transition to.</param>
    public virtual void TransitionTo(QState targetState)
    {
        if (StateMethod != null)
            Trigger(StateMethod, QSignals.Exit);

        SourceStateMethod = StateMethod;
        StateMethod       = targetState;

        if (StateMethod == null)
            return;

        Trigger(StateMethod, QSignals.Entry);
        Trigger(StateMethod, QSignals.Init);

        if (Config.SendStateJobAfterEntry)
            Trigger(StateMethod, QSignals.StateJob);
    }

    protected virtual QState Trigger(QState stateMethod, QSignal qSignal)
    {
        switch (Config.TraceLevel)
        {
            case TraceLevel.All:
                StateEventTrace(stateMethod, qSignal);
                break;

            case TraceLevel.UserSignals:
                // Exclude lifecycle signals
                if (qSignal != QSignals.Entry && qSignal != QSignals.Exit &&
                    qSignal != QSignals.Init && qSignal != QSignals.Empty)
                {
                    StateEventTrace(stateMethod, qSignal);
                }
                break;

            case TraceLevel.None:
            default:
                // No tracing
                break;
        }

        var state = stateMethod.Invoke(new QEvent(qSignal));
        return state;
    }

    protected virtual void StateEventTrace(QState state, QSignal signal) { }

    public virtual void Trace(string s) { }

    /// <summary>
    /// The handler for the top state that is shared by all instances of a QHSM.
    /// </summary>
    /// <param name="qEvent"></param>
    /// <returns></returns>
    public static QState TopState(IQEvent qEvent)
    {
        return null;
    }

    private readonly object _sync = new();
}
