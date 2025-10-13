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

using System.Runtime.CompilerServices;

namespace qf4net;

/// <summary>
/// A class that holds the signals that are intrinsically used by
/// the hierarchical state machine base class <see cref="QHsm"/> and hence are
/// reserved.
/// </summary>
public class QSignals
{
    public static readonly QSignal Empty = new();
    public static readonly QSignal Init = new();
    public static readonly QSignal Entry = new();
    public static readonly QSignal Exit = new();
    public static readonly QSignal Terminate = new();
    public static readonly QSignal StateJob = new();
    public static readonly QSignal Initialized = new();
    public static readonly QSignal Start = new();
    public static readonly QSignal Stop = new();
    public static readonly QSignal Abort = new();
    public static readonly QSignal Pause = new();
    public static readonly QSignal Resume = new();
    public static readonly QSignal Error = new();
    public static readonly QSignal Retry = new();

    public static readonly QEvent EvtEmpty = new(Empty);
    public static readonly QEvent EvtInit = new(Init);
    public static readonly QEvent EvtEntry = new(Entry);
    public static readonly QEvent EvtExit = new(Exit);
    public static readonly QEvent EvtTerminate = new(Terminate);
    public static readonly QEvent EvtStateJob = new(StateJob);
    public static readonly QEvent EvtInitialized = new(Initialized);
    public static readonly QEvent EvtStart = new(Start);
    public static readonly QEvent EvtStop = new(Stop);
    public static readonly QEvent EvtAbort = new(Abort);
    public static readonly QEvent EvtPause = new(Pause);
    public static readonly QEvent EvtResume = new(Resume);
    public static readonly QEvent EvtError = new(Error);
    public static readonly QEvent EvtRetry = new(Retry);
};

/// <summary>
/// QSignal class - an enum replacement.
/// </summary>
public class QSignal : IEquatable<QSignal>, IComparable<QSignal>
{
    public QSignal([CallerMemberName] string name = null)
    {
        Name = name;
        _signalValue = Interlocked.Increment(ref _maxSignalCount);
    }

    public string Name { get; }

    public static explicit operator int(QSignal signal)
    {
        return signal?._signalValue ?? throw new ArgumentNullException(nameof(signal));
    }

    public static bool operator ==(QSignal lhs, QSignal rhs)
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

    public static bool operator !=(QSignal lhs, QSignal rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        return this == (QSignal)obj;
    }

    public override int GetHashCode()
    {
        return _signalValue;
    }

    public override string ToString()
    {
        return $"{Name}:{_signalValue}/{_maxSignalCount}";
    }

    public bool Equals(QSignal other)
    {
        return this == other;
    }

    public int CompareTo(QSignal other)
    {
        return other == null ? 1 : _signalValue.CompareTo(other._signalValue);
    }

    private static int _maxSignalCount;

    private readonly int _signalValue;
}
