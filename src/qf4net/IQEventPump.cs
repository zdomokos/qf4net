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

public interface IQActive : IQEventPump
{

}


/// <summary>
/// Interface that Active Objects implement.
/// </summary>
public interface IQEventPump
{
    /// <summary>
    /// Start the <see cref="IQActive"/> object's thread of execution. The caller needs to assign a
    /// priority to every <see cref="IQActive"/> object in the system. Events will be dispatched according
    /// to the priority assigned to the <see cref="IQActive"/> object.
    /// </summary>
    /// <param name="priority">The priority associated with this <see cref="IQActive"/> object.</param>
    Task RunEventPumpAsync(int priority);
    void RunEventPump(int priority);

    /// <summary>
    /// The priority associated with this <see cref="IQActive"/> object. Once the <see cref="IQActive"/> object
    /// is started the priority is non-negative. For an <see cref="IQActive"/> object that has not yet been started
    /// the value -1 is returned as the priority.
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Post the <see paramref="qEvent"/> directly to the <see cref="IQActive"/> object's event queue
    /// using the FIFO (First In First Out) policy.
    /// </summary>
    /// <param name="qEvent"></param>
    void PostFifo(IQEvent qEvent);

    /// <summary>
    /// Post the <see paramref="qEvent"/> directly to the <see cref="IQActive"/> object's event queue
    /// using the LIFO (Last In First Out) policy.
    /// </summary>
    /// <param name="qEvent"></param>
    void PostLifo(IQEvent qEvent);
}
