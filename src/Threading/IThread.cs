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
using System.Text;

namespace qf4net.Threading
{
	/// <summary>
	/// The interface of a thread
	/// </summary>
	public interface IThread
	{
		/// <summary>
		/// Raises a ThreadAbortException in the thread on which it is invoked, to begin the process
		/// of terminating the thread. Calling this method usually terminates the thread. 
		/// </summary>
		void Abort();

		/// <summary>
		/// Blocks the calling thread until a thread terminates.
		/// </summary>
		void Join();
		
		/// <summary>
		/// Blocks the calling thread until a thread terminates or the specified time elapses. 
		/// </summary>
		/// <param name="millisecondsTimeout"></param>
		/// <returns></returns>
		bool Join(int millisecondsTimeout);

		/// <summary>
		/// Blocks the calling thread until a thread terminates or the specified time elapses.
		/// </summary>
		/// <param name="timeout"></param>
		/// <returns></returns>
		bool Join(TimeSpan timeout);

		/// <summary>
		/// Starts the thread
		/// </summary>
		void Start();

		///// <summary>
		///// Starts the thread
		///// </summary>
		///// <param name="parameter">An object that contains data to be used by the method the thread executes.</param>
		//void Start(object parameter);
	}
}
