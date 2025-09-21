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

namespace qf4net;

/// <summary>
/// QSignal class - an enum replacement.
/// </summary>
public class Signal
{
    public Signal(string name)
    {
        _signalName     = name;
        _signalValue    = _maxSignalCount;
        _maxSignalCount = Interlocked.Increment(ref _maxSignalCount);
    }

    public static int MaxSignalCount => _maxSignalCount;

    public static implicit operator int(Signal sig)
    {
        return sig?._signalValue ?? 0;
    }

    public static bool operator ==(Signal lhs, Signal rhs)
    {
        if (ReferenceEquals(lhs, rhs))
        {
            return true;
        }

        if (lhs is null || rhs is null)
        {
            return false;
        }

        return lhs._signalValue == rhs._signalValue;
    }

    public static bool operator !=(Signal lhs, Signal rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        //Check for null and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        var sig = (Signal)obj;
        return _signalValue == sig._signalValue;
    }

    public override int GetHashCode()
    {
        return _signalValue;
    }

    public override string ToString()
    {
        return _signalName;
    }

    private static int _maxSignalCount;

    private readonly int    _signalValue;
    private readonly string _signalName;
}

public enum SignalId
{
    /// <summary>
    /// Signal that is used to retrieve the super state (must not be used externally).
    /// </summary>
    Empty,

    /// <summary>
    ///
    /// </summary>
    Init,

    /// <summary>
    ///
    /// </summary>
    Entry,

    /// <summary>
    ///
    /// </summary>
    Exit,

    /// <summary>
    /// Entry in the enumeration that marks the first slot that is available for custom signals.
    /// </summary>
    Terminate,
    UserSig,
};

/// <summary>
/// A class that holds the signals that are intrinsically used by
/// the hierarchical state machine base class <see cref="QHsm"/> and hence are
/// reserved.
/// </summary>
public class QSignals
{
    public static readonly Signal Empty     = new(nameof(SignalId.Empty));
    public static readonly Signal Init      = new(nameof(SignalId.Init));
    public static readonly Signal Entry     = new(nameof(SignalId.Entry));
    public static readonly Signal Exit      = new(nameof(SignalId.Exit));
    public static readonly Signal Terminate = new(nameof(SignalId.Terminate));
    public static readonly Signal UserSig   = new("UserSig");
};
