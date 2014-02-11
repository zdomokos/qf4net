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
using System.Threading;
using qf4net;

namespace qf4net
{
	/// <summary>
	/// Thread-safe event queue holding <see cref="QEvent"/> instances.
	/// </summary>
	public class QEventQueue : IQEventQueue
	{
		private LinkedEventList m_EventList;
		
		/// <summary>
		/// Creates a new empty <see cref="QEventQueue"/>
		/// </summary>
		public QEventQueue()
		{
			m_EventList = new LinkedEventList();
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="QEventQueue"/> is empty
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				lock(m_EventList)
				{
					return m_EventList.IsEmpty;
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
				lock(m_EventList)
				{
					return m_EventList.Count;
				}
			}
		}

		/// <summary>
		/// Inserts the <see paramref="qEvent"/> at the end of the queue (First In First Out). 
		/// </summary>
		/// <param name="qEvent"></param>
		public void EnqueueFIFO(IQEvent qEvent)
		{
			lock(m_EventList)
			{
				m_EventList.InsertNewTail(qEvent);
				Monitor.Pulse(m_EventList);
			}
		}

		/// <summary>
		/// Inserts the <see paramref="qEvent"/> at the beginning of the queue (First In First Out). 
		/// </summary>
		/// <param name="qEvent"></param>
		public void EnqueueLIFO(IQEvent qEvent)
		{
			lock(m_EventList)
			{
				m_EventList.InsertNewHead(qEvent);
				Monitor.Pulse(m_EventList);
			}
		}

		/// <summary>
		/// Dequeues the first <see cref="QEvent"/> in the <see cref="QEventQueue"/>. If the <see cref="QEventQueue"/>
		/// is currently empty then it blocks until a new <see cref="QEvent"/> is put into the <see cref="QEventQueue"/>.
		/// </summary>
		/// <returns>The first <see cref="QEvent"/> in the <see cref="QEventQueue"/>.</returns>
		public IQEvent DeQueue()
		{
			lock(m_EventList)
			{
				if (m_EventList.IsEmpty)
				{
					// We wait for the next event to be put into the queue
					Monitor.Wait(m_EventList);
				}

				return m_EventList.RemoveHeadEvent();
			}
		}

		/// <summary>
		/// Allows the caller to peek at the head of the <see cref="QEventQueue"/>.
		/// </summary>
		/// <returns>The <see cref="IQEvent"/> at the head of the <see cref="QEventQueue"/> if it exists; 
		/// otherwise <see langword="null"/></returns>
		public IQEvent Peek()
		{
			lock (m_EventList)
			{
				if (m_EventList.IsEmpty)
				{
					return null;
				}
				else
				{
					return m_EventList.Head.QEvent;
				}
			}
		}

		#region Helper class LinkedEventList

		/// <summary>
		/// Simple single linked list for <see cref="QEvent"/> instances
		/// </summary>
		private class LinkedEventList
		{
			private EventNode m_HeadNode;
			private EventNode m_TailNode;
			private int m_Count;

			internal LinkedEventList()
			{
				m_HeadNode = null;
				m_TailNode = null;
				m_Count = 0;
			}

			internal int Count { get { return m_Count; } }

			internal bool IsEmpty { get { return (m_Count == 0); } }

			internal void InsertNewHead(IQEvent qEvent)
			{
				if (m_Count == 0)
				{
					// We create the first node in the linked list
					m_HeadNode = m_TailNode = new EventNode(qEvent, null);
				}
				else
				{
					EventNode newHead = new EventNode(qEvent, m_HeadNode);
					m_HeadNode = newHead;
				}
				m_Count++;
			}

			internal void InsertNewTail(IQEvent qEvent)
			{
				if (m_Count == 0)
				{
					// We create the first node in the linked list
					m_HeadNode = m_TailNode = new EventNode(qEvent, null);
				}
				else
				{
					EventNode newTail = new EventNode(qEvent, null);
					m_TailNode.NextNode = newTail;
					m_TailNode = newTail; 
				}
				m_Count++;
			}

			internal EventNode Head { get { return m_HeadNode; } }
			internal EventNode Tail { get { return m_TailNode; } }

			/// <summary>
			/// Removes the current head node from the linked list and returns its associated <see cref="QEvent"/>.
			/// </summary>
			/// <returns></returns>
			internal IQEvent RemoveHeadEvent()
			{
				IQEvent qEvent = null;
				if (m_HeadNode != null)
				{
					qEvent = m_HeadNode.QEvent;
					m_HeadNode = m_HeadNode.NextNode;
					m_Count--;
				}
				return qEvent;
			}

			#region Helper class EventNode

			internal class EventNode
			{
				private IQEvent m_QEvent;
				private EventNode m_NextNode;

				internal EventNode(IQEvent qEvent, EventNode nextNode) 
				{
					m_QEvent = qEvent;
					m_NextNode = nextNode;
				}

				internal EventNode NextNode
				{
					get { return m_NextNode; }
					set { m_NextNode = value; }
				}

				internal IQEvent QEvent
				{
					get { return m_QEvent; }
					set { m_QEvent = value; }
				}
			}

			#endregion

		}
		#endregion
	}
}
