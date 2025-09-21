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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

//using System.Windows.Forms;

namespace qf4net
{
    /// <summary>
    /// The base class for all hierarchical state machines
    /// </summary>
    public abstract class QHsm : IQHsm
    {
        private static QState _sTopState;

        /// <summary>
        /// Added for symmetry reasons, so that all deriving classes can add their own static
        /// <see cref="TransitionChainStore"/> variable using the new key word.
        /// </summary>
        protected static TransitionChainStore STransitionChainStore = null;

        private MethodInfo _mMyStateMethod;
        private MethodInfo _mMySourceStateMethod;
        private string _mTargetStateName;

        static QHsm()
        {
            _sTopState = Top;
        }

        /// <summary>
        /// Constructor for the Quantum Hierarchical State Machine.
        /// </summary>
        public QHsm()
        {
            _mMyStateMethod = _sTopState.Method;
        }

        /// <summary>
        /// Getter for an optional <see cref="TransitionChainStore"/> that can hold cached
        /// <see cref="TransitionChain"/> objects that are used to optimize static transitions.
        /// </summary>
        protected virtual TransitionChainStore TransChainStore => null;

        /// <summary>
        /// Is called inside of the function Init to give the deriving class a chance to
        /// initialize the state machine.
        /// </summary>
        protected abstract void InitializeStateMachine();

        /// <summary>
        /// Must only be called once by the client of the state machine to initialize the machine.
        /// </summary>
        public void Init()
        {
            Debug.Assert(_mMyStateMethod == _sTopState.Method); // HSM not executed yet
            MethodInfo stateMethod = _mMyStateMethod; // save m_StateHandler in a temporary

            InitializeStateMachine(); // We call into the deriving class
            // initial transition must go *one* level deep
            Debug.Assert(GetSuperStateMethod(_mMyStateMethod) == stateMethod);

            stateMethod = _mMyStateMethod; // Note: We only use the temporary
            // variable stateMethod so that we can use Assert statements to ensure
            // that each transition is only one level deep.
            Trigger(stateMethod, QSignals.Entry);
            while (Trigger(stateMethod, QSignals.Init) == null) // init handled?
            {
                Debug.Assert(GetSuperStateMethod(_mMyStateMethod) == stateMethod);
                stateMethod = _mMyStateMethod;

                Trigger(stateMethod, QSignals.Entry);
            }
        }

        /// <summary>
        /// Determines whether the state machine is in the state specified by <see paramref="inquiredState"/>.
        /// </summary>
        /// <param name="inquiredState">The state to check for.</param>
        /// <returns>
        /// <see langword="true"/> if the state machine is in the specified state;
        /// <see langword="false"/> otherwise.
        /// </returns>
        /// <remarks>
        /// If the currently active state of a hierarchical state machine is s then it is in the
        /// state s AND all its parent states.
        /// </remarks>
        public bool IsInState(QState inquiredState)
        {
            MethodInfo stateMethod;
            for (
                stateMethod = _mMyStateMethod;
                stateMethod != null;
                stateMethod = GetSuperStateMethod(stateMethod)
            )
            {
                if (stateMethod == inquiredState.Method) // do the states match?
                {
                    return true;
                }
            }
            return false; // no match found
        }

        public MethodInfo StateMethod => _mMyStateMethod;
        public MethodInfo SourceStateMethod => _mMySourceStateMethod;

        /// <summary>
        /// Returns the name of the (deepest) state that the state machine is currently in.
        /// </summary>
        public string CurrentStateName => _mMyStateMethod.Name;

        public string CurrentNestedStateName
        {
            get
            {
                MethodInfo stateMethod;
                string nest = "";
                for (
                    stateMethod = _mMyStateMethod;
                    stateMethod != null;
                    stateMethod = GetSuperStateMethod(stateMethod)
                )
                {
                    if (nest != "")
                        nest = $"[{stateMethod.Name}]->{nest}";
                    else
                        nest = $"[{stateMethod.Name}]";
                }
                return nest;
            }
        }

        /// <summary>
        /// Dispatches the specified event to this state machine
        /// </summary>
        /// <param name="qEvent">The <see cref="IQEvent"/> to dispatch.</param>
        public void Dispatch(IQEvent qEvent)
        {
            // We let the event bubble up the chain until it is handled by a state handler
            try
            {
                int level = 0;
                _mMySourceStateMethod = _mMyStateMethod;
                while (_mMySourceStateMethod != null)
                {
                    StateTrace(_mMySourceStateMethod, qEvent.QSignal, ++level); // ZTG-added
                    QState state = (QState)
                        _mMySourceStateMethod.Invoke(this, new object[] { qEvent });
                    if (state != null)
                    {
                        _mMySourceStateMethod = state.Method;
                    }
                    else
                    {
                        _mMySourceStateMethod = null;
                    }
                }
            }
            catch (TargetInvocationException tie)
            {
                Exception e = tie.InnerException;

                // add the exception history
                string exceptionMessages = "";
                while (e != null)
                {
                    exceptionMessages += $"{e.Message}{Environment.NewLine}";
                    e = e.InnerException;
                }
                string message =
                    $"The following unhandled exception was generated by {this} in {_mTargetStateName}:\n";
                message += exceptionMessages;

                throw new Exception(message);
            }
        }

        /// <summary>
        /// Same as the method <see cref="Dispatch"/> but guarantees that the method can
        /// be executed by only one thread at a time.
        /// </summary>
        /// <param name="qEvent">The <see cref="IQEvent"/> to dispatch.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void DispatchSynchronized(IQEvent qEvent)
        {
            Dispatch(qEvent);
        }

        /// <summary>
        /// The handler for the top state that is shared by all instances of a QHSM.
        /// </summary>
        /// <param name="qEvent"></param>
        /// <returns></returns>
        private static QState Top(IQEvent qEvent)
        {
            return null;
        }

        /// <summary>
        /// The top state of each <see cref="QHsm"/>
        /// </summary>
        protected QState TopState => _sTopState;

        #region Helper functions for the predefined signals

        private MethodInfo Trigger(MethodInfo stateMethod, Signal qSignal)
        {
            StateTrace(stateMethod, qSignal); // ZTG-added
            QState state = (QState)stateMethod.Invoke(this, new object[] { new QEvent(qSignal) });
            if (state == null)
            {
                return null;
            }
            else
            {
                return state.Method;
            }
        }

        /// <summary>
        /// Sends the specified signal to the specified state and (optionally) records the transition
        /// </summary>
        /// <param name="receiverStateMethod">The <see cref="MethodInfo"/> that represents the state method
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
        private MethodInfo Trigger(
            MethodInfo receiverStateMethod,
            Signal qSignal,
            TransitionChainRecorder recorder
        )
        {
            MethodInfo stateMethod = Trigger(receiverStateMethod, qSignal);
            if (stateMethod == null && recorder != null)
            {
                // The receiverState handled the event
                recorder.Record(receiverStateMethod, qSignal);
            }
            return stateMethod;
        }

        ///<summary>
        /// Retrieves the super state (parent state) of the specified
        /// state by sending it the empty signal.
        ///</summary>
        private MethodInfo GetSuperStateMethod(MethodInfo stateMethod)
        {
            QState superState = (QState)
                stateMethod.Invoke(this, new object[] { new QEvent(QSignals.Empty) });
            if (superState != null)
            {
                return superState.Method;
            }
            else
            {
                return null;
            }
        }

        #endregion

        /// <summary>
        /// Represents the macro Q_INIT in Miro Samek's implementation
        /// </summary>
        protected void InitializeState(QState state)
        {
            _mMyStateMethod = state.Method;

            // try setting this in cases of transitionTo before any Dispatches
            _mMySourceStateMethod = _mMyStateMethod;
        }

        /// <summary>
        /// Performs a dynamic transition; i.e., the transition path is determined on the fly and not recorded.
        /// </summary>
        /// <param name="targetState">The <see cref="QState"/> to transition to.</param>
        protected virtual void TransitionTo(QState targetState)
        {
            _mTargetStateName = targetState.Method.Name;

            Debug.Assert(targetState != _sTopState); // can't target 'top' state
            ExitUpToSourceState();
            // This is a dynamic transition. We pass in null instead of a recorder
            TransitionFromSourceToTarget(targetState.Method, null);
        }

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
            Debug.Assert(targetState != _sTopState); // can't target 'top' state
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
        private void TransitionToSynchronized(
            QState targetState,
            ref TransitionChain transitionChain
        )
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
                TransitionChainRecorder recorder = new TransitionChainRecorder();
                TransitionFromSourceToTarget(targetState.Method, recorder);
                // We pass the recorded transition steps back to the caller:
                transitionChain = recorder.GetRecordedTransitionChain();
            }
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

        private void ExitUpToSourceState()
        {
            for (MethodInfo stateMethod = _mMyStateMethod; stateMethod != _mMySourceStateMethod; )
            {
                Debug.Assert(stateMethod != null);
                MethodInfo stateMethodToHandleExit = Trigger(stateMethod, QSignals.Exit);
                if (stateMethodToHandleExit != null)
                {
                    // state did not handle the Exit signal itself
                    stateMethod = stateMethodToHandleExit;
                }
                else
                {
                    // state handled the Exit signal. We need to elicit
                    // the superstate explicitly.
                    stateMethod = GetSuperStateMethod(stateMethod);
                }
            }
        }

        /// <summary>
        /// Handles the transition from the source state to the target state without the help of a previously
        /// recorded transition chain.
        /// </summary>
        /// <param name="targetStateMethod">The <see cref="MethodInfo"/> representing the state method to transition to.</param>
        /// <param name="recorder">An instance of <see cref="TransitionChainRecorder"/> or <see langword="null"/></param>
        /// <remarks>
        /// Passing in <see langword="null"/> as the recorder means that we deal with a dynamic transition.
        /// If an actual instance of <see cref="TransitionChainRecorder"/> is passed in then we deal with a static
        /// transition that was not recorded yet. In this case the function will record the transition steps
        /// as they are determined.
        /// </remarks>
        private void TransitionFromSourceToTarget(
            MethodInfo targetStateMethod,
            TransitionChainRecorder recorder
        )
        {
            List<MethodInfo> statesTargetToLca;
            int indexFirstStateToEnter;
            ExitUpToLca(
                targetStateMethod,
                out statesTargetToLca,
                out indexFirstStateToEnter,
                recorder
            );
            TransitionDownToTargetState(
                targetStateMethod,
                statesTargetToLca,
                indexFirstStateToEnter,
                recorder
            );
        }

        /// <summary>
        /// Determines the transition chain between the target state and the LCA (Least Common Ancestor)
        /// and exits up to LCA while doing so.
        /// </summary>
        /// <param name="targetStateMethod">The target state method of the transition.</param>
        /// <param name="statesTargetToLca">A <see cref="List{MethodInfo}"/> that holds (in reverse order) the states
        /// that need to be entered on the way down to the target state.
        /// Note: The index of the first state that needs to be entered is returned in
        /// <see paramref="indexFirstStateToEnter"/>.</param>
        /// <param name="indexFirstStateToEnter">Returns the index in the array <see cparamref="statesTargetToLCA"/>
        /// that specifies the first state that needs to be entered on the way down to the target state.</param>
        /// <param name="recorder">An instance of <see cref="TransitionChainRecorder"/> if the transition chain
        /// should be recorded; <see langword="null"/> otherwise.</param>
        private void ExitUpToLca(
            MethodInfo targetStateMethod,
            out List<MethodInfo> statesTargetToLca,
            out int indexFirstStateToEnter,
            TransitionChainRecorder recorder
        )
        {
            statesTargetToLca = new List<MethodInfo>();
            statesTargetToLca.Add(targetStateMethod);
            indexFirstStateToEnter = 0;

            // (a) check my source state == target state (transition to self)
            if (_mMySourceStateMethod == targetStateMethod)
            {
                Trigger(_mMySourceStateMethod, QSignals.Exit, recorder);
                return;
            }

            // (b) check my source state == super state of the target state
            MethodInfo targetSuperStateMethod = GetSuperStateMethod(targetStateMethod);
            //Debug.WriteLine(targetSuperStateMethod.Name);
            if (_mMySourceStateMethod == targetSuperStateMethod)
            {
                return;
            }

            // (c) check super state of my source state == super state of target state
            // (most common)
            MethodInfo sourceSuperStateMethod = GetSuperStateMethod(_mMySourceStateMethod);
            if (sourceSuperStateMethod == targetSuperStateMethod)
            {
                Trigger(_mMySourceStateMethod, QSignals.Exit, recorder);
                return;
            }

            // (d) check super state of my source state == target
            if (sourceSuperStateMethod == targetStateMethod)
            {
                Trigger(_mMySourceStateMethod, QSignals.Exit, recorder);
                indexFirstStateToEnter = -1; // we don't enter the LCA
                return;
            }

            // (e) check rest of my source = super state of super state ... of target state hierarchy
            statesTargetToLca.Add(targetSuperStateMethod);
            indexFirstStateToEnter++;
            for (
                MethodInfo stateMethod = GetSuperStateMethod(targetSuperStateMethod);
                stateMethod != null;
                stateMethod = GetSuperStateMethod(stateMethod)
            )
            {
                if (_mMySourceStateMethod == stateMethod)
                {
                    return;
                }

                statesTargetToLca.Add(stateMethod);
                indexFirstStateToEnter++;
            }

            // For both remaining cases we need to exit the source state
            Trigger(_mMySourceStateMethod, QSignals.Exit, recorder);

            // (f) check rest of super state of my source state ==
            //     super state of super state of ... target state
            // The array list is currently filled with all the states
            // from the target state up to the top state
            for (int stateIndex = indexFirstStateToEnter; stateIndex >= 0; stateIndex--)
            {
                if (sourceSuperStateMethod == (MethodInfo)statesTargetToLca[stateIndex])
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
                MethodInfo stateMethod = sourceSuperStateMethod;
                stateMethod != null;
                stateMethod = GetSuperStateMethod(stateMethod)
            )
            {
                for (int stateIndex = indexFirstStateToEnter; stateIndex >= 0; stateIndex--)
                {
                    if (stateMethod == (MethodInfo)statesTargetToLca[stateIndex])
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
            MethodInfo targetStateMethod,
            List<MethodInfo> statesTargetToLca,
            int indexFirstStateToEnter,
            TransitionChainRecorder recorder
        )
        {
            // we enter the states in the passed in array in reverse order
            for (int stateIndex = indexFirstStateToEnter; stateIndex >= 0; stateIndex--)
            {
                Trigger((MethodInfo)statesTargetToLca[stateIndex], QSignals.Entry, recorder);
            }

            _mMyStateMethod = targetStateMethod;

            // At last we are ready to initialize the target state.
            // If the specified target state handles init then the effective
            // target state is deeper than the target state specified in
            // the transition.
            while (Trigger(targetStateMethod, QSignals.Init, recorder) == null)
            {
                // Initial transition must be one level deep
                Debug.Assert(targetStateMethod == GetSuperStateMethod(_mMyStateMethod));
                targetStateMethod = _mMyStateMethod;
                Trigger(targetStateMethod, QSignals.Entry, recorder);
            }

            if (recorder != null)
            {
                // We always make sure that the last entry in the recorder represents the entry to the target state.
                EnsureLastTransistionStepIsEntryIntoTargetState(targetStateMethod, recorder);
                Debug.Assert(recorder.GetRecordedTransitionChain().Length > 0);
            }
        }

        private void EnsureLastTransistionStepIsEntryIntoTargetState(
            MethodInfo targetStateMethod,
            TransitionChainRecorder recorder
        )
        {
            if (recorder.GetRecordedTransitionChain().Length == 0)
            {
                // Nothing recorded so far
                RecordEntryIntoTargetState(targetStateMethod, recorder);
                return;
            }
            else
            {
                // We need to test whether the last recorded transition step is the entry into the target state
                TransitionChain transitionChain = recorder.GetRecordedTransitionChain();
                TransitionStep lastTransitionStep = transitionChain[transitionChain.Length - 1];
                if (
                    lastTransitionStep.StateMethod != targetStateMethod
                    || lastTransitionStep.QSignal != QSignals.Entry
                )
                {
                    RecordEntryIntoTargetState(targetStateMethod, recorder);
                    return;
                }
            }
        }

        private void RecordEntryIntoTargetState(
            MethodInfo targetStateMethod,
            TransitionChainRecorder recorder
        )
        {
            recorder.Record(targetStateMethod, QSignals.Entry);
        }

        private void ExecuteTransitionChain(TransitionChain transitionChain)
        {
            // There must always be at least one transition step in the provided transition chain
            Debug.Assert(transitionChain.Length > 0);

            TransitionStep transitionStep = transitionChain[0]; // to shut up the compiler;
            // without it we would get the following error on the line
            //       m_MyStateMethod = transitionStep.StateMethod;
            // at the end of this method: Use of possibly unassigned field 'State'
            for (int i = 0; i < transitionChain.Length; i++)
            {
                transitionStep = transitionChain[i];
                Trigger(transitionStep.StateMethod, transitionStep.QSignal);
            }
            _mMyStateMethod = transitionStep.StateMethod;
        }

        #region Helper classes for the handling of static transitions

        #region TransitionChainRecorder

        /// <summary>
        /// This class is used to record the individual transition steps that are required to transition from
        /// a given state to a target state.
        /// </summary>
        private class TransitionChainRecorder
        {
            private List<TransitionStep> _mTransitionSteps = new List<TransitionStep>();

            internal void Record(MethodInfo stateMethod, Signal qSignal)
            {
                _mTransitionSteps.Add(new TransitionStep(stateMethod, qSignal));
            }

            /// <summary>
            /// Returns the recorded transition steps in form of a <see cref="TransitionChain"/> instance.
            /// </summary>
            /// <returns></returns>
            internal TransitionChain GetRecordedTransitionChain()
            {
                // We turn the List into a strongly typed array
                return new TransitionChain(_mTransitionSteps);
            }
        }

        #endregion

        #region TransitionChain & TransitionStep

        /// <summary>
        /// Class that wraps the handling of recorded transition steps.
        /// </summary>
        protected class TransitionChain
        {
            private MethodInfo[] _mStateMethodChain;

            //  holds the transitions that need to be performed from the LCA down to the target state

            private BitArray _mActionBits;

            // holds the actions that need to be performed on each transition in two bits:
            // 0x1: Init; 0x2: Entry, 0x3: Exit

            internal TransitionChain(List<TransitionStep> transitionSteps)
            {
                _mStateMethodChain = new MethodInfo[transitionSteps.Count];
                _mActionBits = new BitArray(transitionSteps.Count * 2);

                for (int i = 0; i < transitionSteps.Count; i++)
                {
                    TransitionStep transitionStep = transitionSteps[i];

                    _mStateMethodChain[i] = transitionStep.StateMethod;
                    int bitPos = i * 2;

                    if (QSignals.Empty == transitionStep.QSignal)
                    {
                        _mActionBits[bitPos] = false;
                        _mActionBits[++bitPos] = false;
                    }
                    else if (QSignals.Init == transitionStep.QSignal)
                    {
                        _mActionBits[bitPos] = false;
                        _mActionBits[++bitPos] = true;
                    }
                    else if (QSignals.Entry == transitionStep.QSignal)
                    {
                        _mActionBits[bitPos] = true;
                        _mActionBits[++bitPos] = false;
                    }
                    else if (QSignals.Exit == transitionStep.QSignal)
                    {
                        _mActionBits[bitPos] = true;
                        _mActionBits[++bitPos] = true;
                    }
                }
            }

            internal int Length => _mStateMethodChain.Length;

            internal TransitionStep this[int index]
            {
                get
                {
                    TransitionStep transitionStep = new TransitionStep();
                    transitionStep.StateMethod = _mStateMethodChain[index];

                    int bitPos = index * 2;
                    if (_mActionBits[bitPos])
                    {
                        if (_mActionBits[bitPos + 1])
                        {
                            transitionStep.QSignal = QSignals.Exit;
                        }
                        else
                        {
                            transitionStep.QSignal = QSignals.Entry;
                        }
                    }
                    else
                    {
                        if (_mActionBits[bitPos + 1])
                        {
                            transitionStep.QSignal = QSignals.Init;
                        }
                        else
                        {
                            transitionStep.QSignal = QSignals.Empty;
                        }
                    }
                    return transitionStep;
                }
            }
        }

        internal struct TransitionStep
        {
            internal MethodInfo StateMethod;
            internal Signal QSignal;

            internal TransitionStep(MethodInfo stateMethod, Signal qSignal)
            {
                StateMethod = stateMethod;
                QSignal = qSignal;
            }
        }

        #endregion

        #region TransitionChainStore

        /// <summary>
        /// Class that handles storage and access to the various <see cref="TransitionChain"/> instances
        /// that are required for all the static transitions in use by a given hierarchical state machine.
        /// </summary>
        protected class TransitionChainStore
        {
            private const int CDefaultCapacity = 16;

            private TransitionChain[] _mItems;
            private int _mSize;

            /// <summary>
            /// Constructs a <see cref="TransitionChainStore"/>. The internal array for holding
            /// <see cref="TransitionChain"/> instances is configured to have room for the static
            /// transitions in the base class (if any).
            /// </summary>
            /// <param name="callingClass">The class that called called the constructor.</param>
            public TransitionChainStore(Type callingClass)
            {
                Debug.Assert(IsDerivedFromQHsm(callingClass));

                Type baseType = callingClass.BaseType;
                int slotsRequiredByBaseQHsm = 0;

                while (baseType != typeof(QHsm))
                {
                    slotsRequiredByBaseQHsm += RetrieveStoreSizeOfBaseClass(baseType);
                    baseType = baseType.BaseType;
                }

                InitializeStore(slotsRequiredByBaseQHsm);
            }

            private int RetrieveStoreSizeOfBaseClass(Type baseType)
            {
                BindingFlags bindingFlags =
                    BindingFlags.DeclaredOnly
                    | BindingFlags.NonPublic
                    | BindingFlags.Static
                    | BindingFlags.GetField;

                MemberInfo[] mi = baseType.FindMembers(
                    MemberTypes.Field,
                    bindingFlags,
                    Type.FilterName,
                    "s_TransitionChainStore"
                );

                if (mi.Length < 1)
                {
                    return 0;
                }

                TransitionChainStore store = (TransitionChainStore)
                    baseType.InvokeMember("s_TransitionChainStore", bindingFlags, null, null, null);
                return store.Size;
            }

            private bool IsDerivedFromQHsm(Type type)
            {
                Type baseType = type.BaseType;
                while (baseType != null)
                {
                    if (baseType == typeof(QHsm))
                    {
                        return true;
                    }
                    baseType = baseType.BaseType;
                }

                // None of the base classes is QHsm
                return false;
            }

            private void InitializeStore(int slotsRequiredByBaseQHsm)
            {
                if (slotsRequiredByBaseQHsm == 0)
                {
                    _mItems = new TransitionChain[CDefaultCapacity];
                }
                else
                {
                    _mItems = new TransitionChain[2 * slotsRequiredByBaseQHsm];
                }

                _mSize = slotsRequiredByBaseQHsm;
            }

            /// <summary>
            /// Creates a new slot for a <see cref="TransitionChain"/> and returns its index
            /// </summary>
            /// <returns>The index of the new slot.</returns>
            public int GetOpenSlot()
            {
                if (_mSize >= _mItems.Length)
                {
                    // We no longer have room in the items array to hold a new slot
                    IncreaseCapacity();
                }
                return _mSize++;
            }

            /// <summary>
            /// Reallocates the internal array <see cref="_mItems"/> to an array twice the previous capacity.
            /// </summary>
            private void IncreaseCapacity()
            {
                int newCapacity;
                if (_mItems.Length == 0)
                {
                    newCapacity = CDefaultCapacity;
                }
                else
                {
                    newCapacity = _mItems.Length * 2;
                }
                TransitionChain[] newItems = new TransitionChain[newCapacity];
                Array.Copy(_mItems, 0, newItems, 0, _mItems.Length);
                _mItems = newItems;
            }

            /// <summary>
            /// Should be called once all required slots have been established in order to minimize the memory
            /// footprint of the store.
            /// </summary>
            public void ShrinkToActualSize()
            {
                TransitionChain[] newItems = new TransitionChain[_mSize];
                Array.Copy(_mItems, 0, newItems, 0, _mSize);
                _mItems = newItems;
            }

            /// <summary>
            /// Provides access to the array that holds the persisted <see cref="TransitionChain"/> objects.
            /// </summary>
            public TransitionChain[] TransitionChains => _mItems;

            /// <summary>
            /// The size of the <see cref="TransitionChainStore"/>; i.e., the actual number of used slots.
            /// </summary>
            internal int Size => _mSize;
        }

        #endregion

        #endregion

        #region ZTG-Added
        protected virtual void StateTrace(MethodInfo state, Signal signal, int level = 0) { }
        #endregion
    }
}
