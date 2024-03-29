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
    ///  
    /// </summary>
    public class QEvent : IQEvent
    {
        private readonly Signal _qSignal;

        /// <summary>
        /// Default constructor - initializes all fields to default values
        /// </summary>
        public QEvent(Signal qSignal)
        {
            System.Diagnostics.Debug.Assert((object)qSignal != null, "QEvent created with null signal");
            _qSignal = qSignal;
        }

        /// <summary>
        /// The identifier of the <see cref="QEvent"/> type.
        /// </summary>
        public Signal QSignal => _qSignal;

        /// <summary>
        /// IsSignal
        /// </summary>
        /// <param name="sig"></param>
        /// <returns></returns>
        public bool IsSignal(Signal sig)
        {
            return sig == _qSignal;
        }

        /// <summary>
        /// The QSignal in string form. It allows for simpler debugging and logging. 
        /// </summary>
        /// <returns>The signal as string.</returns>
        public override string ToString()
        {
            return _qSignal.ToString();
        }
        
        public object EventObject { get; set; }

        public override bool Equals(object obj)
        {
            return IsSignal(((QEvent)obj)?._qSignal);
        }

        public override int GetHashCode()
        {
            return _qSignal;
        }
    }
}
