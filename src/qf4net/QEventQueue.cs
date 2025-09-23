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

/// <summary>
/// Thread-safe event queue holding <see cref="QEvent"/> instances.
/// </summary>
public class QEventQueue : IQEventQueue
{
    private readonly LinkedEventList _eventList;

    /// <summary>
    /// Creates a new empty <see cref="QEventQueue"/>
    /// </summary>
    public QEventQueue()
    {
        _eventList = new LinkedEventList();
    }

    /// <summary>
    /// Returns <see langword="true"/> if the <see cref="QEventQueue"/> is empty
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            lock (_eventList)
            {
                return _eventList.IsEmpty;
            }
        }
    }

    /// <summary>
    /// Number of events in the queue
    /// </summary>
    public int Count
    {
        get
        {
            lock (_eventList)
            {
                return _eventList.Count;
            }
        }
    }

    /// <summary>
    /// Inserts the <see paramref="qEvent"/> at the end of the queue (First In First Out).
    /// </summary>
    /// <param name="qEvent"></param>
    public void EnqueueFifo(IQEvent qEvent)
    {
        lock (_eventList)
        {
            _eventList.InsertNewTail(qEvent);
            Monitor.Pulse(_eventList);
        }
    }

    /// <summary>
    /// Inserts the <see paramref="qEvent"/> at the beginning of the queue (First In First Out).
    /// </summary>
    /// <param name="qEvent"></param>
    public void EnqueueLifo(IQEvent qEvent)
    {
        lock (_eventList)
        {
            _eventList.InsertNewHead(qEvent);
            Monitor.Pulse(_eventList);
        }
    }

    /// <summary>
    /// Dequeues the first <see cref="QEvent"/> in the <see cref="QEventQueue"/>. If the <see cref="QEventQueue"/>
    /// is currently empty then it blocks until a new <see cref="QEvent"/> is put into the <see cref="QEventQueue"/>.
    /// </summary>
    /// <returns>The first <see cref="QEvent"/> in the <see cref="QEventQueue"/>.</returns>
    public IQEvent DeQueue()
    {
        lock (_eventList)
        {
            if (_eventList.IsEmpty)
            {
                // We wait for the next event to be put into the queue
                Monitor.Wait(_eventList);
            }

            return _eventList.RemoveHeadEvent();
        }
    }

    /// <summary>
    /// Allows the caller to peek at the head of the <see cref="QEventQueue"/>.
    /// </summary>
    /// <returns>The <see cref="IQEvent"/> at the head of the <see cref="QEventQueue"/> if it exists;
    /// otherwise <see langword="null"/></returns>
    public IQEvent Peek()
    {
        lock (_eventList)
        {
            if (_eventList.IsEmpty)
            {
                return null;
            }

            return _eventList.Head.QEvent;
        }
    }

    #region Helper class LinkedEventList

    /// <summary>
    /// Simple single linked list for <see cref="QEvent"/> instances
    /// </summary>
    private class LinkedEventList
    {
        internal LinkedEventList()
        {
            Head  = null;
            Tail  = null;
            Count = 0;
        }

        internal int Count { get; private set; }

        internal bool IsEmpty => Count == 0;

        internal void InsertNewHead(IQEvent qEvent)
        {
            if (Count == 0)
            {
                // We create the first node in the linked list
                Head = Tail = new EventNode(qEvent, null);
            }
            else
            {
                var newHead = new EventNode(qEvent, Head);
                Head = newHead;
            }
            Count++;
        }

        internal void InsertNewTail(IQEvent qEvent)
        {
            if (Count == 0)
            {
                // We create the first node in the linked list
                Head = Tail = new EventNode(qEvent, null);
            }
            else
            {
                var newTail = new EventNode(qEvent, null);
                Tail.NextNode = newTail;
                Tail          = newTail;
            }
            Count++;
        }

        internal EventNode Head { get; private set; }

        internal EventNode Tail { get; private set; }

        /// <summary>
        /// Removes the current head node from the linked list and returns its associated <see cref="QEvent"/>.
        /// </summary>
        /// <returns></returns>
        internal IQEvent RemoveHeadEvent()
        {
            IQEvent qEvent = null;
            if (Head != null)
            {
                qEvent = Head.QEvent;
                Head   = Head.NextNode;
                Count--;
            }
            return qEvent;
        }

        #region Helper class EventNode

        internal class EventNode
        {
            internal EventNode(IQEvent qEvent, EventNode nextNode)
            {
                QEvent   = qEvent;
                NextNode = nextNode;
            }

            internal EventNode NextNode { get; set; }
            internal IQEvent   QEvent   { get; set; }
        }

        #endregion
    }
    #endregion
}
