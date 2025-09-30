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

/// <summary>
/// Provides timer functionality for posting events to an associated <see cref="IQEventPump"/>
/// after specified time intervals. Supports both one-shot and periodic timer operations.
/// </summary>
public class QTimer : IDisposable
{
    /// <summary>
    /// Creates a new <see cref="QTimer"/> instance.
    /// </summary>
    /// <param name="qActive">The <see cref="IQEventPump"/> object that owns this <see cref="QTimer"/>; this is also
    /// the <see cref="IQEventPump"/> object that will receive the timer-based events.</param>
    public QTimer(IQEventPump qActive)
    {
        _qActive = qActive;
        _timer = new Timer(
                           OnTimer,
                           null,             // we don't need a state object
                           Timeout.Infinite, // don't start yet
                           Timeout.Infinite  // no periodic firing
                          );
    }

    /// <summary>
    /// Releases all resources used by the <see cref="QTimer"/>.
    /// </summary>
    public void Dispose()
    {
        _timer?.Dispose();
    }

    /// <summary>
    /// Arms the <see cref="QTimer"/> to perform a one-time timeout.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to wait before the timeout occurs. Must be positive.</param>
    /// <param name="qEvent">The <see cref="IQEvent"/> to post into the associated <see cref="IQEventPump"/>
    /// object when the timeout occurs.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="timeSpan"/> is not positive.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="qEvent"/> is null.</exception>
    public void FireIn(TimeSpan timeSpan, IQEvent qEvent)
    {
        if (!(timeSpan > TimeSpan.Zero))
            throw new ArgumentException("The provided timespan must be positive", nameof(timeSpan));
        ArgumentNullException.ThrowIfNull(qEvent);

        lock (_sync)
        {
            _qEvent = qEvent;
            _timer.Change((long)timeSpan.TotalMilliseconds,  Timeout.Infinite);
        }
    }

    /// <summary>
    /// Arms the <see cref="QTimer"/> to perform periodic timeouts at regular intervals.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> interval between individual timeouts. Must be positive.</param>
    /// <param name="qEvent">The <see cref="IQEvent"/> to post into the associated <see cref="IQEventPump"/>
    /// object each time a timeout occurs.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="timeSpan"/> is not positive.</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="qEvent"/> is null.</exception>
    public void FireEvery(TimeSpan timeSpan, IQEvent qEvent)
    {
        if (!(timeSpan > TimeSpan.Zero))
            throw new ArgumentException("The provided timespan must be positive", nameof(timeSpan));
        ArgumentNullException.ThrowIfNull(qEvent);

        lock (_sync)
        {
            _qEvent = qEvent;
            _timer.Change(timeSpan, timeSpan);
        }
    }

    /// <summary>
    /// Disarms the timer, preventing any future timeout events from being posted.
    /// </summary>
    public void Disarm()
    {
        lock (_sync)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            // Since a timer performs the callback on a thread of the thread pool we could have a race condition
            // between stopping (disarming) a timer and the callback being invoked afterwards.
            // We circumvent this problem by changing the event to null.
            _qEvent = null;
        }
    }

    /// <summary>
    /// Rearms the timer as a one-shot timer using the previously configured event.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to wait before the timeout occurs. Must be positive.</param>
    /// <exception cref="ArgumentNullException">Thrown when no event has been previously configured.</exception>
    public void Rearm(TimeSpan timeSpan)
    {
        if (!(timeSpan > TimeSpan.Zero))
            throw new ArgumentException("The provided timespan must be positive", nameof(timeSpan));

        lock (_sync)
        {
            if (_qEvent == null)
                throw new ArgumentNullException(nameof(_qEvent), "No event has been previously configured. Call FireIn or FireEvery first.");
            
            _timer.Change((long)timeSpan.TotalMilliseconds,  Timeout.Infinite);
        }
    }

    /// <summary>
    /// Callback method invoked by the underlying <see cref="Timer"/> when a timeout occurs.
    /// Posts the configured event to the associated event pump if one is set.
    /// </summary>
    /// <param name="state">Timer state object (unused).</param>
    private void OnTimer(object state)
    {
        if (_qEvent == null)
            return;

        lock (_sync)
        {
            _qActive.PostFifo(_qEvent);
        }
    }

    private readonly IQEventPump _qActive;
    private readonly Timer       _timer;
    private readonly object      _sync = new();
    private          IQEvent     _qEvent;
}
