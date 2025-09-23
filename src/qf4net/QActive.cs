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

using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace qf4net;


public abstract class QActive : QHsm, IQActive
{
    protected QActive()
    {
        _messagePump = new QEventPump(this, HsmUnhandledException, EventLoopTerminated);
    }

    public Task RunEventPumpAsync(int priority)
    {
        return _messagePump.RunEventPumpAsync(priority);
    }

    public void RunEventPump(int priority)
    {
        _messagePump.RunEventPump(priority);
    }

    public int Priority => _messagePump.Priority;

    public void PostFifo(IQEvent qEvent)
    {
        _messagePump.PostFifo(qEvent);
    }

    public void PostLifo(IQEvent qEvent)
    {
        _messagePump.PostLifo(qEvent);
    }

    protected abstract void HsmUnhandledException(Exception e);
    protected virtual  void EventLoopTerminated(IQEventPump obj) { }

    private readonly QEventPump _messagePump;
}
