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


namespace qf4net
{
	/// <summary>
	/// Interface implemented by the QF
	/// </summary>
	public interface IQf
	{
		/// <summary>
		/// Initializes the the quantum framework. Must be called exactly once before any of the other methods on
		/// <see cref="IQf"/> can be used.
		/// </summary>
		/// <param name="maxSignal">The maximal signal that the <see cref="IQf"/> must be able to handle.</param>
		void Initialize(int maxSignal);

		/// <summary>
		/// Allows an <see cref="IQActive"/> object to subscribe for a given signal.
		/// </summary>
		/// <param name="qActive">The subscribing <see cref="IQActive"/> object.</param>
		/// <param name="qSignal">The signal to subscribe for.</param>
		void Subscribe(IQActive qActive, Signal qSignal);

		/// <summary>
		/// Allows an <see cref="IQActive"/> object to unsubscribe for a given signal.
		/// </summary>
		/// <param name="qActive">The unsubscribing <see cref="IQActive"/> object.</param>
		/// <param name="qSignal">The signal to unsubscribe.</param>
		void Unsubscribe(IQActive qActive, Signal qSignal);

		/// <summary>
		/// Allows an event source to publish an event. 
		/// </summary>
		/// <param name="qEvent">The <see cref="QEvent"/> to publish.</param>
		void Publish(QEvent qEvent);
	}
}
