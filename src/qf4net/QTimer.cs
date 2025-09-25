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
/// Summary description for QTimer.
/// </summary>
public class QTimer : IDisposable
{
    /// <summary>
    /// Creates a new <see cref="QTimer"/> instance.
    /// </summary>
    /// <param name="qActive">The <see cref="IQActive"/> object that owns this <see cref="QTimer"/>; this is also
    /// the <see cref="IQActive"/> object that will receive the timer based events.</param>
    public QTimer(IQActive qActive)
    {
        _qActive = qActive;
        _timer = new Timer(
            OnTimer,
            null, // we don't need a state object
            Timeout.Infinite, // don't start yet
            Timeout.Infinite // no periodic firing
        );
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    /// <summary>
    /// Arms the <see cref="QTimer"/> to perform a one-time timeout.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to wait before the timeout occurs.</param>
    /// <param name="qEvent">The <see cref="IQEvent"/> to post into the associated <see cref="IQEventPump"/>
    /// object when the timeout occurs.</param>
    public void FireIn(TimeSpan timeSpan, IQEvent qEvent)
    {
        if (!(timeSpan > TimeSpan.Zero))
        {
            throw new ArgumentException("The provided timespan must be positive", nameof(timeSpan));
        }
        ArgumentNullException.ThrowIfNull(qEvent);

        lock (_timer)
        {
            _qEvent = qEvent;
            _timer.Change(timeSpan, new TimeSpan(-1));
        }
    }

    /// <summary>
    /// Arms the <see cref="QTimer"/> to perform a periodic timeout.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> interval between individual timeouts.</param>
    /// <param name="qEvent">The <see cref="IQEvent"/> to post into the associated <see cref="IQEventPump"/>
    /// object when the timeout occurs.</param>
    public void FireEvery(TimeSpan timeSpan, IQEvent qEvent)
    {
        ArgumentNullException.ThrowIfNull(qEvent);

        lock (_timer)
        {
            if (!(timeSpan > TimeSpan.Zero))
            {
                throw new ArgumentException(
                    "The provided timespan must be positive",
                    nameof(timeSpan)
                );
            }

            _qEvent = qEvent;
            _timer.Change(timeSpan, timeSpan);
        }
    }

    /// <summary>
    /// Disarms the timer.
    /// </summary>
    public void Disarm()
    {
        lock (_timer)
        {
            _timer.Change(Timeout.Infinite, Timeout.Infinite);
            // Since a timer performs the callback on a thread of the thread pool we could have a race condition
            // between stopping (disarming) a timer and the callback being invoked afterwards.
            // We circumvent this problem by changing the event to null.
            _qEvent = null;
        }
    }

    /// <summary>
    /// Rearms the timer as a one-shot timer.
    /// </summary>
    /// <param name="timeSpan">The <see cref="TimeSpan"/> to wait before the timeout occurs.</param>
    public void Rearm(TimeSpan timeSpan)
    {
        ArgumentNullException.ThrowIfNull(_qEvent);

        lock (_timer)
        {
            _timer.Change(timeSpan, new TimeSpan(-1));
        }
    }

    /// <summary>
    /// Callback for the timer event
    /// </summary>
    /// <param name="state"></param>
    private void OnTimer(object state)
    {
        lock (_timer)
        {
            if (_qEvent != null)
            {
                _qActive.PostFifo(_qEvent);
            }
        }
    }

    private readonly IQEventPump _qActive;
    private readonly Timer _timer;
    private IQEvent _qEvent;
}
