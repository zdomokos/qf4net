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
using System.Runtime.CompilerServices;

namespace qf4net
{
    /// <summary>
    /// QF singleton. This is the class that handles the delivery of events.
    /// </summary>
    public class Qf : IQf
    {
        // Implementation of not-quite lazy, but thread-safe singleton pattern without using locks
        // See http://www.yoda.arachsys.com/csharp/singleton.html for details
        private static readonly Qf SInstance = new Qf(); // holds reference to the singleton instance
        private SortedList[] _mSignalSubscribers;

        // Explicit static constructor to tell C# compiler
        // not to mark type as beforefieldinit
        static Qf()
        {
        }

        private Qf()
        {
        }

        /// <summary>
        /// Allows a client application to get the instance of the singleton <see cref="IQf"/>.
        /// </summary>
        /// <returns>Reference to the singleton <see cref="IQf"/> instance.</returns>
        public static IQf Instance => SInstance;

        #region IQF Members

        /// <summary>
        /// Initializes the the quantum framework. Must be called exactly once before any of the other methods on
        /// <see cref="IQf"/> can be used.
        /// </summary>
        /// <param name="maxSignal">The maximal signal that the <see cref="IQf"/> must be able to handle.</param>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Initialize(int maxSignal)
        {
            if (_mSignalSubscribers != null)
            {
                // Initialize was called before
                throw new InvalidOperationException("The Initialize method can only be called once.");
            }
            _mSignalSubscribers = new SortedList[maxSignal + 1];
        }

        /// <summary>
        /// Allows an <see cref="IQActive"/> object to subscribe for a given signal.
        /// </summary>
        /// <param name="qActive">The subscribing <see cref="IQActive"/> object.</param>
        /// <param name="qSignal">The signal to subscribe for.</param>
        public void Subscribe(IQActive qActive, Signal qSignal)
        {
            //Debug.WriteLine(qActive.ToString() + " subscribes for signal " + qSignal.ToString());
            lock (_mSignalSubscribers)
            {
                if (_mSignalSubscribers[qSignal] == null)
                {
                    // this is the first time that somebody subscribes for this signal
                    _mSignalSubscribers[qSignal] = new SortedList();
                }

                _mSignalSubscribers[qSignal].Add(qActive.Priority, qActive);
            }
        }

        /// <summary>
        /// Allows an <see cref="IQActive"/> object to unsubscribe for a given signal.
        /// </summary>
        /// <param name="qActive">The unsubscribing <see cref="IQActive"/> object.</param>
        /// <param name="qSignal">The signal to unsubscribe.</param>
        public void Unsubscribe(IQActive qActive, Signal qSignal)
        {
            lock (_mSignalSubscribers)
            {
                _mSignalSubscribers[qSignal].Remove(qActive.Priority);
            }
        }

        /// <summary>
        /// Allows an event source to publish an event. 
        /// </summary>
        /// <param name="qEvent">The <see cref="QEvent"/> to publish.</param>
        public void Publish(QEvent qEvent)
        {
            lock (_mSignalSubscribers)
            {
                if (qEvent.QSignal < _mSignalSubscribers.Length)
                {
                    SortedList sortedSubscriberList = _mSignalSubscribers[qEvent.QSignal];
                    if (sortedSubscriberList != null)
                    {
                        // For simplicity we do not use the event propagae pattern that Miro Samek uses in his implementation.
                        // This has two consequences:
                        // a) We rely on the Garbage Collector to clean up events that are no longer used
                        // b) We don't have the restriction that only once instance of a given type (signal value) can be in use at any given time
                        for (int i = 0; i < sortedSubscriberList.Count; i++)
                        {
                            IQActive subscribingQActive = (IQActive)sortedSubscriberList.GetByIndex(i);
                            subscribingQActive.PostFifo(qEvent);
                        }
                    }
                }
            }
        }

        #endregion

        #region Helper class SubscriberList

        /// <summary>
        /// This class encapsulates the storage of the inforamtion about subscribers for a given event type (signal) 
        /// </summary>
        private class SubscriberList
        {
            private SortedList _mSubscriberList;

            internal SubscriberList()
            {
                _mSubscriberList = new SortedList();
            }

            internal void AddSubscriber(IQActive qActive)
            {
                _mSubscriberList.Add(qActive.Priority, qActive);
            }

            internal void RemoveSubscriber(IQActive qActive)
            {
                _mSubscriberList.Remove(qActive.Priority);
            }
        }

        #endregion
    }
}
