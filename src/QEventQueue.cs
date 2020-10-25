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


using System.Threading;

namespace qf4net
{
	/// <summary>
	/// Thread-safe event queue holding <see cref="QEvent"/> instances.
	/// </summary>
	public class QEventQueue : IQEventQueue
	{
		private LinkedEventList _mEventList;
		
		/// <summary>
		/// Creates a new empty <see cref="QEventQueue"/>
		/// </summary>
		public QEventQueue()
		{
			_mEventList = new LinkedEventList();
		}

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="QEventQueue"/> is empty
		/// </summary>
		public bool IsEmpty
		{
			get
			{
				lock(_mEventList)
				{
					return _mEventList.IsEmpty;
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
				lock(_mEventList)
				{
					return _mEventList.Count;
				}
			}
		}

		/// <summary>
		/// Inserts the <see paramref="qEvent"/> at the end of the queue (First In First Out). 
		/// </summary>
		/// <param name="qEvent"></param>
		public void EnqueueFifo(IQEvent qEvent)
		{
			lock(_mEventList)
			{
				_mEventList.InsertNewTail(qEvent);
				Monitor.Pulse(_mEventList);
			}
		}

		/// <summary>
		/// Inserts the <see paramref="qEvent"/> at the beginning of the queue (First In First Out). 
		/// </summary>
		/// <param name="qEvent"></param>
		public void EnqueueLifo(IQEvent qEvent)
		{
			lock(_mEventList)
			{
				_mEventList.InsertNewHead(qEvent);
				Monitor.Pulse(_mEventList);
			}
		}

		/// <summary>
		/// Dequeues the first <see cref="QEvent"/> in the <see cref="QEventQueue"/>. If the <see cref="QEventQueue"/>
		/// is currently empty then it blocks until a new <see cref="QEvent"/> is put into the <see cref="QEventQueue"/>.
		/// </summary>
		/// <returns>The first <see cref="QEvent"/> in the <see cref="QEventQueue"/>.</returns>
		public IQEvent DeQueue()
		{
			lock(_mEventList)
			{
				if (_mEventList.IsEmpty)
				{
					// We wait for the next event to be put into the queue
					Monitor.Wait(_mEventList);
				}

				return _mEventList.RemoveHeadEvent();
			}
		}

		/// <summary>
		/// Allows the caller to peek at the head of the <see cref="QEventQueue"/>.
		/// </summary>
		/// <returns>The <see cref="IQEvent"/> at the head of the <see cref="QEventQueue"/> if it exists; 
		/// otherwise <see langword="null"/></returns>
		public IQEvent Peek()
		{
			lock (_mEventList)
			{
				if (_mEventList.IsEmpty)
				{
					return null;
				}
				else
				{
					return _mEventList.Head.QEvent;
				}
			}
		}

		#region Helper class LinkedEventList

		/// <summary>
		/// Simple single linked list for <see cref="QEvent"/> instances
		/// </summary>
		private class LinkedEventList
		{
			private EventNode _mHeadNode;
			private EventNode _mTailNode;
			private int _mCount;

			internal LinkedEventList()
			{
				_mHeadNode = null;
				_mTailNode = null;
				_mCount = 0;
			}

			internal int Count => _mCount;

			internal bool IsEmpty => _mCount == 0;

			internal void InsertNewHead(IQEvent qEvent)
			{
				if (_mCount == 0)
				{
					// We create the first node in the linked list
					_mHeadNode = _mTailNode = new EventNode(qEvent, null);
				}
				else
				{
					EventNode newHead = new EventNode(qEvent, _mHeadNode);
					_mHeadNode = newHead;
				}
				_mCount++;
			}

			internal void InsertNewTail(IQEvent qEvent)
			{
				if (_mCount == 0)
				{
					// We create the first node in the linked list
					_mHeadNode = _mTailNode = new EventNode(qEvent, null);
				}
				else
				{
					EventNode newTail = new EventNode(qEvent, null);
					_mTailNode.NextNode = newTail;
					_mTailNode = newTail; 
				}
				_mCount++;
			}

			internal EventNode Head => _mHeadNode;
			internal EventNode Tail => _mTailNode;

			/// <summary>
			/// Removes the current head node from the linked list and returns its associated <see cref="QEvent"/>.
			/// </summary>
			/// <returns></returns>
			internal IQEvent RemoveHeadEvent()
			{
				IQEvent qEvent = null;
				if (_mHeadNode != null)
				{
					qEvent = _mHeadNode.QEvent;
					_mHeadNode = _mHeadNode.NextNode;
					_mCount--;
				}
				return qEvent;
			}

			#region Helper class EventNode

			internal class EventNode
			{
				private IQEvent _mQEvent;
				private EventNode _mNextNode;

				internal EventNode(IQEvent qEvent, EventNode nextNode) 
				{
					_mQEvent = qEvent;
					_mNextNode = nextNode;
				}

				internal EventNode NextNode
				{
					get => _mNextNode;
					set => _mNextNode = value;
				}

				internal IQEvent QEvent
				{
					get => _mQEvent;
					set => _mQEvent = value;
				}
			}

			#endregion

		}
		#endregion
	}
}
