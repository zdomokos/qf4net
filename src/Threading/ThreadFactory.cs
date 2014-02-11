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
using System.Runtime.CompilerServices;
using System.Threading;

namespace qf4net.Threading
{
	/// <summary>
	/// Helper class that encapsulates the creation of threads. By routing all
	/// thread requests through this class we have one point in the system where
	/// we can switch between various thread strategies like priority based
	/// thread pools.
	/// </summary>
	public class ThreadFactory
	{
		private static IThreadFactory s_ThreadFactory = new DefaultThreadFactory();
		//private static IThreadFactory s_ThreadFactory = new ImpersonatingThreadFactory();

		/// <summary>
		/// Can be called to specify the <see cref="IThreadFactory"/> instance
		/// that should be used to create new <see cref="Thread"/>s. If not called
		/// then the <see cref="DefaultThreadFactory"/> will be used to create
		/// <see cref="Thread"/>s.
		/// </summary>
		/// <param name="threadFactory">The <see cref="IThreadFactory"/> instance
		/// to use for creating new <see cref="Thread"/>s.</param>
		[MethodImpl(MethodImplOptions.Synchronized)]
		public static void Initialize(IThreadFactory threadFactory)
		{
			s_ThreadFactory = threadFactory;
		}
		
		/// <summary>
		/// Hands out a <see cref="Thread"/> instance.
		/// </summary>
		/// <param name="priority">The priority for the thread to be handed out.</param>
		/// <param name="start">The <see cref="ThreadStart"/> delegate pointing to the method that the thread will start on.</param>
		/// <returns></returns>
		public static IThread GetThread(int priority, ThreadStart start)
		{
			return s_ThreadFactory.GetThread(priority, start);
		}
	}
}
