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

namespace qf4net
{
	/// <summary>
	/// Summary description for IEventQueue.
	/// </summary>
	public interface IQEventQueue
	{
		/// <summary>
		/// Inserts the <see paramref="qEvent"/> at the end of the queue (First In First Out). 
		/// </summary>
		/// <param name="qEvent"></param>
		void EnqueueFIFO(IQEvent qEvent);

		/// <summary>
		/// Inserts the <see paramref="qEvent"/> at the beginning of the queue (First In First Out). 
		/// </summary>
		/// <param name="qEvent"></param>
		void EnqueueLIFO(IQEvent qEvent);

		/// <summary>
		/// Returns <see langword="true"/> if the <see cref="IQEventQueue"/> is empty
		/// </summary>
		bool IsEmpty { get; }

		/// <summary>
		/// Number of events in the queue
		/// </summary>
		int Count { get; }

		/// <summary>
		/// Dequeues the first <see cref="IQEvent"/> in the <see cref="IQEventQueue"/>. If the <see cref="IQEventQueue"/>
		/// is currently empty then it blocks until the a new <see cref="IQEvent"/> is put into the <see cref="IQEventQueue"/>.
		/// </summary>
		/// <returns>The first <see cref="IQEvent"/> in the <see cref="IQEventQueue"/>.</returns>
		IQEvent DeQueue();

	}
}
