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
using System.Threading;
using System.Security.Principal;

namespace qf4net.Threading
{
	/// <summary>
	/// This class provides a wrapper around <see cref="Thread"/> that handles the flaw in the implementation of
	/// <see cref="Thread"/> that causes it to not correctly impersonate the identity of the caller that 
	/// constructs the <see cref="Thread"/> on Windows 2000.
	/// </summary>
	internal class ImpersonatingThread : IThread
	{
		private Thread m_WrappedThread;
		private IntPtr m_SecurityToken;
		private ThreadStart m_ThreadStart;
		//private ParameterizedThreadStart m_ParameterizedThreadStart;

		/// <summary>
		/// Creates a new <see cref="ImpersonatingThread"/>
		/// </summary>
		/// <param name="start"></param>
		public ImpersonatingThread(ThreadStart start)
		{
			if (start == null)
			{
				throw new ArgumentNullException("start");
			}

			m_WrappedThread = new Thread(new ThreadStart(this.InternalStart));
			m_ThreadStart = start;
		}

		///// <summary>
		///// Creates a new <see cref="ImpersonatingThread"/>
		///// </summary>
		///// <param name="start"></param>
		//public ImpersonatingThread(ParameterizedThreadStart start)
		//{
		//    if (start == null)
		//    {
		//        throw new ArgumentNullException("start");
		//    }

		//    m_WrappedThread = new Thread(new ParameterizedThreadStart(this.InternalParameterizedStart));
		//    m_ParameterizedThreadStart = start;
		//}

		/// <summary>
		/// Starts the thread
		/// </summary>
		public void Start()
		{
			InitializeSecurityToken();
			m_WrappedThread.Start();
		}

		///// <summary>
		///// Starts the thread
		///// </summary>
		///// <param name="parameter">An object that contains data to be used by the method the thread executes.</param>
		//public void Start(object parameter)
		//{
		//    InitializeSecurityToken();
		//    m_WrappedThread.Start(parameter);
		//}

		private void InitializeSecurityToken()
		{
			m_SecurityToken = WindowsIdentity.GetCurrent().Token;
			// We don't need to duplicate this token since it is already a token that can be used for impersonation
			// purposes (in constrast to the primary token of the process)
			// See http://www.pluralsight.com/wiki/default.aspx/Keith.GuideBook.HowToCreateAWindowsPrincipalGivenAToken
			// for details.

			//IntPtr originalToken = WindowsIdentity.GetCurrent().Token;
			//s_Log.Debug("In InitializeSecurityToken with original token " + originalToken.ToString());
			//if (!Win32Api.DuplicateToken(originalToken, Win32Api.SecurityImpersonationLevel.SecurityImpersonation, ref m_SecurityToken))
			//{
			//    throw new Win32Exception(Marshal.GetLastWin32Error());
			//}
			//s_Log.Debug("In InitializeSecurityToken with duplicated token " + m_SecurityToken.ToString());
		}

		private void InternalStart()
		{
			WindowsImpersonationContext impersonatedUser = null;
			try
			{
				//try
				//{
					WindowsIdentity tempWindowsIdentity = new WindowsIdentity(m_SecurityToken);
					impersonatedUser = tempWindowsIdentity.Impersonate();
				//}
				//catch (Exception ex)
				//{
				//    s_Log.Error("Failed to impersonate user from the security token " + m_SecurityToken.ToString(), ex);
				//    throw;
				//}
				m_ThreadStart();
			}
			finally
			{
				if (impersonatedUser != null)
				{
					impersonatedUser.Undo();
				}
			}
		}

		//private void InternalParameterizedStart(object obj)
		//{
		//    WindowsImpersonationContext impersonatedUser = null;
		//    try
		//    {
		//        //try
		//        //{
		//            WindowsIdentity tempWindowsIdentity = new WindowsIdentity(m_SecurityToken);
		//            impersonatedUser = tempWindowsIdentity.Impersonate();
		//        //}
		//        //catch (Exception ex)
		//        //{
		//        //    s_Log.Error("Failed to impersonate user from the security token " + m_SecurityToken.ToString(), ex);
		//        //    throw;
		//        //}
		//        m_ParameterizedThreadStart(obj);
		//    }
		//    finally
		//    {
		//        if (impersonatedUser != null)
		//        {
		//            impersonatedUser.Undo();
		//        }
		//    }
		//}

		/// <summary>
		/// Blocks the calling thread until a thread terminates.
		/// </summary>
		public void Join()
		{
			m_WrappedThread.Join();
		}

		/// <summary>
		/// Blocks the calling thread until a thread terminates or the specified time elapses. 
		/// </summary>
		/// <param name="millisecondsTimeout"></param>
		/// <returns></returns>
		public bool Join(int millisecondsTimeout)
		{
			return m_WrappedThread.Join(millisecondsTimeout);
		}

		/// <summary>
		/// Blocks the calling thread until a thread terminates or the specified time elapses.
		/// </summary>
		/// <param name="timeout"></param>
		/// <returns></returns>
		public bool Join(TimeSpan timeout)
		{
			return m_WrappedThread.Join(timeout);
		}

		/// <summary>
		/// Raises a ThreadAbortException in the thread on which it is invoked, to begin the process
		/// of terminating the thread. Calling this method usually terminates the thread. 
		/// </summary>
		public void Abort()
		{
			m_WrappedThread.Abort();
		}
	}
}
