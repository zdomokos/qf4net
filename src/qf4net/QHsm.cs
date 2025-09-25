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

using System.Diagnostics;
using System.Reflection;

namespace qf4net;

/// <summary>
/// The base class for all hierarchical state machines (HSMs)
/// </summary>
public class QHsm: QFsm
{
    /// <summary>
    /// Constructor for the Quantum State Machine.
    /// </summary>
    protected QHsm(StatemachineConfig fsmConfig = null) : base(fsmConfig)
    {
    }

    /// <summary>
    /// Must only be called once by the client of the state machine to initialize the machine.
    /// </summary>
    public override void Init()
    {
        Debug.Assert(StateMethod == _sTopState); // HSM not executed yet

        InitializeStateMachine(); // We call into the deriving class

        var stateMethod = StateMethod;           // save m_StateHandler in a temporary

        // initial transition must go *one* level deep
        Debug.Assert(GetSuperStateMethod(StateMethod) == stateMethod);

        // variable stateMethod so that we can use Assert statements to ensure
        // that each transition is only one level deep.
        Trigger(stateMethod, QSignals.Entry);

        while (Trigger(stateMethod, QSignals.Init) == null) // init handled?
        {
            Debug.Assert(GetSuperStateMethod(StateMethod) == stateMethod);

            stateMethod = StateMethod;

            Trigger(stateMethod, QSignals.Entry);
        }
    }

    public override bool IsInState(QState inquiredState)
    {
        QState stateMethod;
        for (stateMethod = StateMethod; stateMethod != null; stateMethod = GetSuperStateMethod(stateMethod))
        {
            if (stateMethod == inquiredState) // do the states match?
            {
                return true;
            }
        }

        return false; // no match found
    }

    public string CurrentNestedStateName
    {
        get
        {
            QState stateMethod;
            var    nest = "";
            for (
                    stateMethod = StateMethod;
                    stateMethod != null;
                    stateMethod = GetSuperStateMethod(stateMethod)
                )
            {
                nest = nest != "" ? $"[{stateMethod.Method.Name}]->{nest}" : $"[{stateMethod.Method.Name}]";
            }

            return nest;
        }
    }

    ///<summary>
    /// Retrieves the super state (parent state) of the specified
    /// state by sending it the empty signal.
    ///</summary>
    protected QState GetSuperStateMethod(QState stateMethod)
    {
        var superState = stateMethod?.Invoke(new QEvent(QSignals.Empty));
        return superState;
    }

    /// <summary>
    /// Dispatches the specified event to this state machine
    /// </summary>
    /// <param name="qEvent">The <see cref="IQEvent"/> to dispatch.</param>
    public override void Dispatch(IQEvent qEvent)
    {
        try
        {
            var level = 0;
            SourceStateMethod = StateMethod;

            if (SourceStateMethod != null)
            {
                StateTrace(SourceStateMethod, qEvent.Signal, ++level);
                var state = (QState)SourceStateMethod.Method.Invoke(this, [qEvent]);
                SourceStateMethod = state;
            }
        }
        catch (TargetInvocationException tie)
        {
            var e = tie.InnerException;
            var exceptionMessages = "";
            while (e != null)
            {
                exceptionMessages += $"{e.Message}{Environment.NewLine}";
                e = e.InnerException;
            }

            var message = $"The following unhandled exception was generated by {this}:\n";
            message += exceptionMessages;

            throw new Exception(message);
        }
    }

    /// <summary>
    /// Performs a dynamic transition; i.e., the transition path is determined on the fly.
    /// </summary>
    /// <param name="targetState">The <see cref="QState"/> to transition to.</param>
    protected override void TransitionTo(QState targetState)
    {
        // _targetStateName = targetState.Method.Name;

        Debug.Assert(targetState != _sTopState); // can't target 'top' state

        ExitUpToSourceState();
        TransitionFromSourceToTarget(targetState);
    }

    protected void ExitUpToSourceState()
    {
        for (var stateMethod = StateMethod; stateMethod != SourceStateMethod;)
        {
            Debug.Assert(stateMethod != null);

            var stateMethodToHandleExit = Trigger(stateMethod, QSignals.Exit);
            // state did not handle the Exit signal itself
            stateMethod = stateMethodToHandleExit ??
                          // state handled the Exit signal. We need to elicit superstate explicitly.
                          GetSuperStateMethod(stateMethod);
        }
    }

    /// <summary>
    /// Handles the transition from the source state to the target state.
    /// </summary>
    /// <param name="targetStateMethod">The <see cref="QState"/> representing the state method to transition to.</param>
    private void TransitionFromSourceToTarget(QState targetStateMethod)
    {
        ExitUpToLca(targetStateMethod, out var statesTargetToLca, out var indexFirstStateToEnter);
        TransitionDownToTargetState(targetStateMethod, statesTargetToLca, indexFirstStateToEnter);
    }

    /// <summary>
    /// Determines the transition chain between the target state and the LCA (Least Common Ancestor)
    /// and exits up to LCA while doing so.
    /// </summary>
    /// <param name="targetStateMethod">The target state method of the transition.</param>
    /// <param name="statesTargetToLca">A <see cref="List{QState}"/> that holds (in reverse order) the states
    /// that need to be entered on the way down to the target state.
    /// Note: The index of the first state that needs to be entered is returned in
    /// <see paramref="indexFirstStateToEnter"/>.</param>
    /// <param name="indexFirstStateToEnter">Returns the index in the array <see cparamref="statesTargetToLCA"/>
    /// that specifies the first state that needs to be entered on the way down to the target state.</param>
    private void ExitUpToLca(QState targetStateMethod, out List<QState> statesTargetToLca, out int indexFirstStateToEnter)
    {
        statesTargetToLca      = [targetStateMethod];
        indexFirstStateToEnter = 0;

        // (a) check my source state == target state (transition to self)
        if (SourceStateMethod == targetStateMethod)
        {
            Trigger(SourceStateMethod, QSignals.Exit);
            return;
        }

        // (b) check my source state == super state of the target state
        var targetSuperStateMethod = GetSuperStateMethod(targetStateMethod);
        //Console.WriteLine(targetSuperStateMethod.Name);
        if (SourceStateMethod == targetSuperStateMethod)
        {
            return;
        }

        // (c) check super state of my source state == super state of target state
        // (most common)
        var sourceSuperStateMethod = GetSuperStateMethod(SourceStateMethod);
        if (sourceSuperStateMethod == targetSuperStateMethod)
        {
            Trigger(SourceStateMethod, QSignals.Exit);
            return;
        }

        // (d) check super state of my source state == target
        if (sourceSuperStateMethod == targetStateMethod)
        {
            Trigger(SourceStateMethod, QSignals.Exit);
            indexFirstStateToEnter = -1; // we don't enter the LCA
            return;
        }

        // (e) check rest of my source = super state of super state ... of target state hierarchy
        statesTargetToLca.Add(targetSuperStateMethod);
        indexFirstStateToEnter++;
        for (
                var stateMethod = GetSuperStateMethod(targetSuperStateMethod);
                stateMethod != null;
                stateMethod = GetSuperStateMethod(stateMethod)
            )
        {
            if (SourceStateMethod == stateMethod)
            {
                return;
            }

            statesTargetToLca.Add(stateMethod);
            indexFirstStateToEnter++;
        }

        // For both remaining cases we need to exit the source state
        Trigger(SourceStateMethod, QSignals.Exit);

        // (f) check rest of super state of my source state ==
        //     super state of super state of ... target state
        // The array list is currently filled with all the states
        // from the target state up to the top state
        for (var stateIndex = indexFirstStateToEnter; stateIndex >= 0; stateIndex--)
        {
            if (sourceSuperStateMethod == statesTargetToLca[stateIndex])
            {
                indexFirstStateToEnter = stateIndex - 1;
                // Note that we do not include the LCA state itself;
                // i.e., we do not enter the LCA
                return;
            }
        }

        // (g) check each super state of super state ... of my source state ==
        //     super state of super state of ... target state
        for (
                var stateMethod = sourceSuperStateMethod;
                stateMethod != null;
                stateMethod = GetSuperStateMethod(stateMethod)
            )
        {
            for (var stateIndex = indexFirstStateToEnter; stateIndex >= 0; stateIndex--)
            {
                if (stateMethod == statesTargetToLca[stateIndex])
                {
                    indexFirstStateToEnter = stateIndex - 1;
                    // Note that we do not include the LCA state itself;
                    // i.e., we do not enter the LCA
                    return;
                }
            }

            Trigger(stateMethod, QSignals.Exit);
        }

        // We should never get here
        throw new InvalidOperationException("Mal formed Hierarchical State Machine");
    }

    private void TransitionDownToTargetState(QState targetStateMethod, List<QState> statesTargetToLca, int indexFirstStateToEnter)
    {
        for (var stateIndex = indexFirstStateToEnter; stateIndex >= 0; stateIndex--)
        {
            Trigger(statesTargetToLca[stateIndex], QSignals.Entry);
        }

        StateMethod = targetStateMethod;

        // At last, we are ready to initialize the target state.
        // If the specified target state handles init then the effective
        // target state is deeper than the target state specified in
        // the transition.
        while (Trigger(targetStateMethod, QSignals.Init) == null)
        {
            // Initial transition must be one level deep
            Debug.Assert(targetStateMethod == GetSuperStateMethod(StateMethod));
            targetStateMethod = StateMethod;
            Trigger(targetStateMethod, QSignals.Entry);
        }
    }
}
