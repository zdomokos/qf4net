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
using System.Runtime.CompilerServices;

namespace qf4net;

public class QHsmLegacy : QHsmWithTransitionChains;

/// <summary>
/// The base class for all hierarchical state machines that support static transitions
/// </summary>
public class QHsmWithTransitionChains : QHsm
{
    /// <summary>
    /// Added for symmetry reasons, so that all deriving classes can add their own static
    /// <see cref="TransitionChainStore"/> variable using the new key word.
    /// </summary>
    protected static TransitionChainStore STransitionChainStore = null;

    /// <summary>
    /// Constructor for the Quantum Hierarchical State Machine.
    /// </summary>
    protected QHsmWithTransitionChains() { }

    /// <summary>
    /// Getter for an optional <see cref="TransitionChainStore"/> that can hold cached
    /// <see cref="TransitionChain"/> objects that are used to optimize static transitions.
    /// </summary>
    protected virtual TransitionChainStore TransChainStore => STransitionChainStore;

    #region Helper functions for the predefined signals

    /// <summary>
    /// Sends the specified signal to the specified state and (optionally) records the transition
    /// </summary>
    /// <param name="receiverStateMethod">The <see cref="QState"/> that represents the state method
    /// to which to send the signal.</param>
    /// <param name="qSignal">The <see cref="QSignals"/> to send.</param>
    /// <param name="recorder">An instance of <see cref="TransitionChainRecorder"/> if the transition
    /// is to be recorded; <see langword="null"/> otherwise.</param>
    /// <returns>The <see cref="QState"/> returned by the state that recieved the signal.</returns>
    /// <remarks>
    /// Even if a recorder is specified, the transition will only be recorded if the state
    /// <see paramref="receiverStateMethod"/> actually handled it.
    /// This function is used to record the transition chain for a static transition that is executed
    /// the first time.
    /// </remarks>
    private QState Trigger(QState receiverStateMethod, QSignal qSignal, TransitionChainRecorder recorder)
    {
        var stateMethod = Trigger(receiverStateMethod, qSignal);
        if (stateMethod == null && recorder != null)
        {
            // The receiverState handled the event
            recorder.Record(receiverStateMethod, qSignal);
        }

        return stateMethod;
    }

    #endregion


    /// <summary>
    /// Performs the transition from the current state to the specified target state.
    /// </summary>
    /// <param name="targetState">The <see cref="QState"/> to transition to.</param>
    /// <param name="transitionChain">A <see cref="TransitionChain"/> used to hold the transition chain that
    /// needs to be executed to perform the transition to the target state.</param>
    /// <remarks>
    /// The very first time that a given static transition is executed, the <see paramref="transitionChain"/>
    /// reference will point to <see langword="null"/>. In this case a new <see cref="TransitionChain"/>
    /// instance is created. As the complete transition is performed the individual transition steps are
    /// recorded in the new <see cref="TransitionChain"/> instance. At the end of the call the new
    /// (and now filled) <see cref="TransitionChain"/> is handed back to the caller.
    /// If the same transition needs to be performed later again, the caller needs to pass
    /// in the filled <see cref="TransitionChain"/> instance. The recorded transition path will then be
    /// played back very efficiently.
    /// </remarks>
    protected void TransitionTo(QState targetState, ref TransitionChain transitionChain)
    {
        Debug.Assert(targetState != TopState); // can't target 'top' state

        ExitUpToSourceState();

        if (transitionChain == null) // for efficiency the first check is not thread-safe
        {
            // We implement the double-checked locking pattern
            TransitionToSynchronized(targetState, ref transitionChain);
        }
        else
        {
            // We just need to 'replay' the transition chain that is stored in the transitions chain.
            ExecuteTransitionChain(transitionChain);
        }
    }

    [MethodImpl(MethodImplOptions.Synchronized)]
    private void TransitionToSynchronized(QState targetState, ref TransitionChain transitionChain)
    {
        if (transitionChain != null)
        {
            // We encountered a race condition. The first (non-synchronized) check indicated that the transition chain
            // is null. However, a second threat beat us in getting into this synchronized method and populated
            // the transition chain in the meantime. We can execute the regular method again now.
            TransitionTo(targetState, ref transitionChain);
        }
        else
        {
            // The transition chain is not initialized yet, we need to dynamically retrieve
            // the required transition steps and record them so that we can subsequently simply
            // play them back.
            var recorder = new TransitionChainRecorder();
            TransitionFromSourceToTarget(targetState, recorder);
            // We pass the recorded transition steps back to the caller:
            transitionChain = recorder.GetRecordedTransitionChain();
        }
    }

    /// <summary>
    /// Handles the transition from the source state to the target state without the help of a previously
    /// recorded transition chain.
    /// </summary>
    /// <param name="targetStateMethod">The <see cref="QState"/> representing the state method to transition to.</param>
    /// <param name="recorder">An instance of <see cref="TransitionChainRecorder"/> or <see langword="null"/></param>
    /// <remarks>
    /// Passing in <see langword="null"/> as the recorder means that we deal with a dynamic transition.
    /// If an actual instance of <see cref="TransitionChainRecorder"/> is passed in then we deal with a static
    /// transition that was not recorded yet. In this case the function will record the transition steps
    /// as they are determined.
    /// </remarks>
    private void TransitionFromSourceToTarget(QState targetStateMethod, TransitionChainRecorder recorder)
    {
        ExitUpToLca(targetStateMethod, out var statesTargetToLca, out var indexFirstStateToEnter, recorder);
        TransitionDownToTargetState(targetStateMethod, statesTargetToLca, indexFirstStateToEnter, recorder);
    }

    /// <summary>
    /// Performs a static transition from the current state to the specified target state. The
    /// <see cref="TransitionChain"/> that specifies the steps required for the static transition
    /// is specified by the provided index into the <see cref="TransitionChainStore"/>. Note that this
    /// method can only be used if the class that implements the <see cref="QHsm"/> provides a class
    /// specific <see cref="TransitionChainStore"/> via the virtual getter <see cref="TransChainStore"/>.
    /// </summary>
    /// <param name="targetState">The <see cref="QState"/> to transition to.</param>
    /// <param name="chainIndex">The index into <see cref="TransitionChainStore"/> pointing to the
    /// <see cref="TransitionChain"/> that is used to hold the individual transition steps that are
    /// required to perform the transition.</param>
    /// <remarks>
    /// In order to use the method the calling class must retrieve the chain index during its static
    /// construction phase by calling the method <see cref="TransitionChainStore.GetOpenSlot()"/> on
    /// its static <see cref="TransitionChainStore"/>.
    /// </remarks>
    protected void TransitionTo(QState targetState, int chainIndex)
    {
        // This method can only be used if a TransitionChainStore has been created for the QHsm
        Debug.Assert(TransChainStore != null);

        TransitionTo(targetState, ref TransChainStore.TransitionChains[chainIndex]);
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
    /// <param name="recorder">An instance of <see cref="TransitionChainRecorder"/> if the transition chain
    /// should be recorded; <see langword="null"/> otherwise.</param>
    private void ExitUpToLca(
        QState targetStateMethod,
        out List<QState> statesTargetToLca,
        out int indexFirstStateToEnter,
        TransitionChainRecorder recorder
    )
    {
        statesTargetToLca = [targetStateMethod];
        indexFirstStateToEnter = 0;

        // (a) check my source state == target state (transition to self)
        if (SourceStateMethod == targetStateMethod)
        {
            Trigger(SourceStateMethod, QSignals.Exit, recorder);
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
            Trigger(SourceStateMethod, QSignals.Exit, recorder);
            return;
        }

        // (d) check super state of my source state == target
        if (sourceSuperStateMethod == targetStateMethod)
        {
            Trigger(SourceStateMethod, QSignals.Exit, recorder);
            indexFirstStateToEnter = -1; // we don't enter the LCA
            return;
        }

        // (e) check rest of my source = super state of super state ... of target state hierarchy
        statesTargetToLca.Add(targetSuperStateMethod);
        indexFirstStateToEnter++;
        for (var stateMethod = GetSuperStateMethod(targetSuperStateMethod); stateMethod != null; stateMethod = GetSuperStateMethod(stateMethod))
        {
            if (SourceStateMethod == stateMethod)
            {
                return;
            }

            statesTargetToLca.Add(stateMethod);
            indexFirstStateToEnter++;
        }

        // For both remaining cases we need to exit the source state
        Trigger(SourceStateMethod, QSignals.Exit, recorder);

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
        for (var stateMethod = sourceSuperStateMethod; stateMethod != null; stateMethod = GetSuperStateMethod(stateMethod))
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

            Trigger(stateMethod, QSignals.Exit, recorder);
        }

        // We should never get here
        throw new InvalidOperationException("Mal formed Hierarchical State Machine");
    }

    private void TransitionDownToTargetState(
        QState targetStateMethod,
        List<QState> statesTargetToLca,
        int indexFirstStateToEnter,
        TransitionChainRecorder recorder
    )
    {
        // we enter the states in the passed in array in reverse order
        for (var stateIndex = indexFirstStateToEnter; stateIndex >= 0; stateIndex--)
        {
            Trigger(statesTargetToLca[stateIndex], QSignals.Entry, recorder);
        }

        StateMethod = targetStateMethod;

        // At last, we are ready to initialize the target state.
        // If the specified target state handles init then the effective
        // target state is deeper than the target state specified in
        // the transition.
        while (Trigger(targetStateMethod, QSignals.Init, recorder) == null)
        {
            // Initial transition must be one level deep
            Debug.Assert(targetStateMethod == GetSuperStateMethod(StateMethod));
            targetStateMethod = StateMethod;
            Trigger(targetStateMethod, QSignals.Entry, recorder);
        }

        if (recorder != null)
        {
            // We always make sure that the last entry in the recorder represents the entry to the target state.
            EnsureLastTransitionStepIsEntryIntoTargetState(targetStateMethod, recorder);
            Debug.Assert(recorder.GetRecordedTransitionChain().Length > 0);
        }
    }

    private void EnsureLastTransitionStepIsEntryIntoTargetState(QState targetStateMethod, TransitionChainRecorder recorder)
    {
        if (recorder.GetRecordedTransitionChain().Length == 0)
        {
            // Nothing recorded so far
            RecordEntryIntoTargetState(targetStateMethod, recorder);
            return;
        }

        // We need to test whether the last recorded transition step is the entry into the target state
        var transitionChain = recorder.GetRecordedTransitionChain();
        var lastTransitionStep = transitionChain[transitionChain.Length - 1];
        if (lastTransitionStep.StateMethod != targetStateMethod || lastTransitionStep.QSignal != QSignals.Entry)
        {
            RecordEntryIntoTargetState(targetStateMethod, recorder);
        }
    }

    private void RecordEntryIntoTargetState(QState targetStateMethod, TransitionChainRecorder recorder)
    {
        recorder.Record(targetStateMethod, QSignals.Entry);
    }

    private void ExecuteTransitionChain(TransitionChain transitionChain)
    {
        // There must always be at least one transition step in the provided transition chain
        Debug.Assert(transitionChain.Length > 0);

        var transitionStep = transitionChain[0]; // to shut up the compiler;
        // without it we would get the following error on the line
        //       m_MyStateMethod = transitionStep.StateMethod;
        // at the end of this method: Use of possibly unassigned field 'State'
        for (var i = 0; i < transitionChain.Length; i++)
        {
            transitionStep = transitionChain[i];
            Trigger(transitionStep.StateMethod, transitionStep.QSignal);
        }

        StateMethod = transitionStep.StateMethod;
    }
}

public abstract class QActiveLegacy : QHsmLegacy, IQActive
{
    protected QActiveLegacy()
    {
        _messagePump = new QEventPump(this, HsmUnhandledException, EventLoopTerminated);
    }

    public Task RunEventPumpAsync(int priority, CancellationToken cancellationToken = default)
    {
        return _messagePump.RunEventPumpAsync(priority, cancellationToken);
    }

    public void RunEventPump(int priority, CancellationToken cancellationToken = default)
    {
        _messagePump.RunEventPump(priority, cancellationToken);
    }

    public int Priority => _messagePump.Priority;

    public void PostFifo(IQEvent qEvent)
    {
        _messagePump.PostFifo(qEvent);
    }

    public void PostLifo(IQEvent qEvent)
    {
        _messagePump.PostLifo(qEvent);
    }

    protected abstract void HsmUnhandledException(Exception e);

    protected virtual void EventLoopTerminated(IQEventPump obj) { }

    private readonly QEventPump _messagePump;
}
