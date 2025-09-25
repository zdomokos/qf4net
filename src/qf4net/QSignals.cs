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

using System.Collections.Concurrent;

namespace qf4net;

/// <summary>
/// A class that holds the signals that are intrinsically used by
/// the hierarchical state machine base class <see cref="QHsm"/> and hence are
/// reserved.
/// </summary>
public class QSignals
{
    public static readonly Signal Empty       = new(nameof(Empty));
    public static readonly Signal Init        = new(nameof(Init));
    public static readonly Signal Entry       = new(nameof(Entry));
    public static readonly Signal Exit        = new(nameof(Exit));
    public static readonly Signal Terminate   = new(nameof(Terminate));

    public static readonly Signal StateJob    = new(nameof(StateJob));
    public static readonly Signal Initialized = new(nameof(Initialized));
    public static readonly Signal Start       = new(nameof(Start));
    public static readonly Signal Stop        = new(nameof(Stop));
    public static readonly Signal Abort       = new(nameof(Abort));
    public static readonly Signal Pause       = new(nameof(Pause));
    public static readonly Signal Resume      = new(nameof(Resume));
    public static readonly Signal Error       = new(nameof(Error));
    public static readonly Signal Retry       = new(nameof(Retry));

    public static readonly QEvent EvtEmpty       = new(Empty);
    public static readonly QEvent EvtInit        = new(Init);
    public static readonly QEvent EvtEntry       = new(Entry);
    public static readonly QEvent EvtExit        = new(Exit);
    public static readonly QEvent EvtTerminate   = new(Terminate);
    public static readonly QEvent EvtStateJob    = new(StateJob);
    public static readonly QEvent EvtInitialized = new(Initialized);
    public static readonly QEvent EvtStart       = new(Start);
    public static readonly QEvent EvtStop        = new(Stop);
    public static readonly QEvent EvtAbort       = new(Abort);
    public static readonly QEvent EvtPause       = new(Pause);
    public static readonly QEvent EvtResume      = new(Resume);
    public static readonly QEvent EvtError       = new(Error);
    public static readonly QEvent EvtRetry       = new(Retry);
};

/// <summary>
/// QSignal class - an enum replacement with name tracking and duplicate prevention.
/// </summary>
public class Signal : IEquatable<Signal>, IComparable<Signal>
{
    private static readonly ConcurrentDictionary<string, Signal> _registeredSignals = new();
    private static int _maxSignalCount;

    /// <summary>
    /// Creates a new Signal with the specified name.
    /// Throws an exception if a signal with the same name already exists.
    /// </summary>
    /// <param name="name">The unique name for this signal</param>
    /// <exception cref="ArgumentNullException">Thrown when name is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when a signal with this name already exists</exception>
    public Signal(string name)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(name);

        _signalName = name;
        _signalValue = Interlocked.Increment(ref _maxSignalCount);

        // Attempt to register this signal, throw if duplicate exists
        if (!_registeredSignals.TryAdd(name, this))
        {
            throw new InvalidOperationException($"A signal with name '{name}' is already registered");
        }
    }

    /// <summary>
    /// Gets the name of this signal.
    /// </summary>
    public string Name => _signalName;

    /// <summary>
    /// Gets the unique numeric value of this signal.
    /// </summary>
    public int Value => _signalValue;

    /// <summary>
    /// Gets a signal by name if it exists.
    /// </summary>
    /// <param name="name">The name of the signal to retrieve</param>
    /// <returns>The signal if found, null otherwise</returns>
    public static Signal GetByName(string name)
    {
        return string.IsNullOrWhiteSpace(name) ? null : _registeredSignals.GetValueOrDefault(name);
    }

    /// <summary>
    /// Checks if a signal with the specified name exists.
    /// </summary>
    /// <param name="name">The name to check</param>
    /// <returns>True if a signal with this name exists, false otherwise</returns>
    public static bool Exists(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && _registeredSignals.ContainsKey(name);
    }

    /// <summary>
    /// Gets all registered signal names.
    /// </summary>
    public static IReadOnlyCollection<string> RegisteredNames => _registeredSignals.Keys.ToArray();

    /// <summary>
    /// Gets all registered signals.
    /// </summary>
    public static IReadOnlyCollection<Signal> RegisteredSignals => _registeredSignals.Values.ToArray();

    /// <summary>
    /// Gets the total count of registered signals.
    /// </summary>
    public static int Count => _registeredSignals.Count;

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
        return this == (Signal)obj;
    }

    public override int GetHashCode()
    {
        return _signalValue;
    }

    public override string ToString()
    {
        return _signalName;
    }

    /// <summary>
    /// Gets a detailed string representation of the signal including its value and count.
    /// </summary>
    public string ToDetailedString()
    {
        return $"{_signalName}:{_signalValue}/{_maxSignalCount}";
    }

    public bool Equals(Signal other)
    {
        return this == other;
    }

    public int CompareTo(Signal other)
    {
        return other == null ? 1 : _signalValue.CompareTo(other._signalValue);
    }

    private readonly int    _signalValue;
    private readonly string _signalName;
}
