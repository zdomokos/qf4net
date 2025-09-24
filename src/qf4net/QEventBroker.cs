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

namespace qf4net;

public class QEventBrokerSingleton
{
    private static readonly Lazy<IQEventBroker> _instance = new(() => new QEventBroker());

    public static IQEventBroker Instance => _instance.Value;
}


/// <summary>
/// QF singleton. Object should be injected into QActive
/// This is the class that handles the delivery of events (publish-subscribe) between
/// active hsm objects.
/// </summary>
public class QEventBroker : IQEventBroker
{
    /// <summary>
    /// This class encapsulates the storage of the information about subscribers for a given event type (signal)
    /// </summary>
    private class SignalSubscribersByPriorityList : SortedDictionary<int, List<IQActive>>;

    /// <summary>
    /// Allows an <see cref="IQActive"/> object to subscribe for a given signal.
    /// </summary>
    /// <param name="qActive">The subscribing <see cref="IQActive"/> object.</param>
    /// <param name="qSignal">The signal to subscribe for.</param>
    public void Subscribe(IQActive qActive, Signal qSignal)
    {
        Console.WriteLine(qActive + " subscribes for signal " + qSignal);

        lock(_syncObj)
        {
            if (!_signalSubscribers.TryGetValue(qSignal, out var subscriptionPriorityList))
            {
                subscriptionPriorityList = [];
                _signalSubscribers[qSignal] = subscriptionPriorityList;
            }
            if (!subscriptionPriorityList.TryGetValue(qActive.Priority, out var subscribersAtPriority))
            {
                subscribersAtPriority = [];
                subscriptionPriorityList[qActive.Priority] = subscribersAtPriority;
            }
            subscribersAtPriority.Add(qActive);
        }
    }

    /// <summary>
    /// Allows an <see cref="IQActive"/> object to unsubscribe for a given signal.
    /// </summary>
    /// <param name="qActive">The unsubscribing <see cref="IQActive"/> object.</param>
    /// <param name="qSignal">The signal to unsubscribe.</param>
    public void Unsubscribe(IQActive qActive, Signal qSignal)
    {
        lock(_syncObj)
        {
            if (_signalSubscribers.TryGetValue(qSignal, out var subscriptionPriorityList) &&
                subscriptionPriorityList.TryGetValue(qActive.Priority, out var subscribersAtPriority))
            {
                subscribersAtPriority.Remove(qActive);
                if (subscribersAtPriority.Count == 0)
                {
                    subscriptionPriorityList.Remove(qActive.Priority);
                }
            }
        }
    }

    public void Unregister(IQActive qActive)
    {
        lock (_syncObj)
        {
            foreach(var subscribers in _signalSubscribers.Values.ToList())
            {
                foreach(var priorityGroup in subscribers.ToList())
                {
                    var subscribersAtPriority = priorityGroup.Value;
                    if (subscribersAtPriority.Remove(qActive))
                    {
                        if (subscribersAtPriority.Count == 0)
                        {
                            subscribers.Remove(priorityGroup.Key);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Allows an event source to publish an event.
    /// </summary>
    /// <param name="qEvent">The <see cref="QEvent"/> to publish.</param>
    public void Publish(IQEvent qEvent)
    {
        lock(_syncObj)
        {
            if(_signalSubscribers.TryGetValue(qEvent.Signal, out var sortedSubscriberList))
            {
                // For simplicity, we do not use the event propagate pattern that Miro Samek uses in his implementation.
                // This has two consequences:
                // a) We rely on the Garbage Collector to clean up events that are no longer used
                // b) We don't have the restriction that only once instance of a given type (signal value) can be in use at any given time
                foreach (var priorityGroup in sortedSubscriberList)
                {
                    foreach (var subscribingQEventPump in priorityGroup.Value)
                    {
                        subscribingQEventPump.PostFifo(qEvent);
                    }
                }
            }
        }
    }

    private readonly Dictionary<Signal, SignalSubscribersByPriorityList> _signalSubscribers = [];
    private readonly object _syncObj = new();
}
