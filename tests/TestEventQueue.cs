using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using qf4net;

namespace UnitTests
{
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    [TestFixture]
    public class TestEventQueue
    {
        [Test]
        public void TestFifoAndLifoOrder()
        {
            int numQEvents = 5;
            QEvent[] qEvents = new QEvent[numQEvents];
            for (int i = 0; i < numQEvents; i++)
            {
                qEvents[i] = new QEvent(new Signal(i.ToString()));
            }

            IQEventQueue eventQueue = new QEventQueue();
            eventQueue.EnqueueFifo(qEvents[2]);
            eventQueue.EnqueueLifo(qEvents[1]);
            eventQueue.EnqueueFifo(qEvents[3]);
            eventQueue.EnqueueLifo(qEvents[0]);
            eventQueue.EnqueueFifo(qEvents[4]);
            Assert.Equals(5, eventQueue.Count); // "Number of events in the queue"

            // when we dequeue the events they should come back in the original order
            for (int i = 0; i < numQEvents; i++)
            {
                IQEvent qEvent = eventQueue.DeQueue();
                Assert.Equals(i, qEvent.QSignal); // "Expected signal id"
                Assert.Equals(5 - i - 1, eventQueue.Count); // "Number of events in the queue"
            }
        }

        [Test]
        public void TestBlockingOnEmptyQueue()
        {
            IQEventQueue eventQueue = new QEventQueue();
            EventLoop eventLoop = new EventLoop(eventQueue);

            int numQEvents = 3;
            QEvent[] qEvents = new QEvent[numQEvents];
            for (int i = 0; i < numQEvents; i++)
            {
                qEvents[i] = new QEvent(new Signal(i.ToString()));
                eventQueue.EnqueueFifo(qEvents[i]);
                Thread.Sleep(10); // We give the event loop a chance to execute
            }

            eventQueue.EnqueueFifo(new QEvent(new Signal("Stop"))); // We send a 'stop' signal
            eventLoop.Join(); // Wait until the event loop is done

            QEvent[] handledEvents = eventLoop.HandledEvents;

            // we check that the events were handled in the correct order
            for (int i = 0; i < numQEvents; i++)
            {
                Assert.Equals(qEvents[i], handledEvents[i]); //"Expected QEvent"
            }
        }

        #region Helper class EventLoop

        private class EventLoop
        {
            private readonly List<QEvent> _handledEvents;
            private readonly IQEventQueue _eventQueue;
            private readonly Thread _workerThread;

            internal EventLoop(IQEventQueue eventQueue)
            {
                _handledEvents = new List<QEvent>();
                _eventQueue = eventQueue;
                _workerThread = new Thread(new ThreadStart(this.HandleEvents));
                _workerThread.Start();
            }

            internal void HandleEvents()
            {
                while (true)
                {
                    lock (_handledEvents)
                    {
                        IQEvent qEvent = _eventQueue.DeQueue();
                        if (qEvent.QSignal == -1)
                        {
                            // we use a signal value of -1 to indicate that the loop should stop
                            break;
                        }

                        _handledEvents.Add((QEvent)qEvent);
                    }
                }
            }

            /// <summary>
            /// Blocks the calling thread until the loop exited the event loop and ended its thread
            /// </summary>
            internal void Join()
            {
                _workerThread.Join();
            }

            internal QEvent[] HandledEvents
            {
                get
                {
                    lock (_handledEvents)
                    {
                        return _handledEvents.ToArray();
                    }
                }
            }

        #endregion
        }
    }
}
