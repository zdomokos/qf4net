using System;
using System.Threading;
using System.Collections;
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
			for(int i = 0; i < numQEvents; i++)
			{
				qEvents[i] = new QEvent(i);
			}

			IQEventQueue eventQueue = new QEventQueue();
			eventQueue.EnqueueFIFO(qEvents[2]);
			eventQueue.EnqueueLIFO(qEvents[1]);
			eventQueue.EnqueueFIFO(qEvents[3]);
			eventQueue.EnqueueLIFO(qEvents[0]);
			eventQueue.EnqueueFIFO(qEvents[4]);
			Assertion.AssertEquals("Number of events in the queue", 5, eventQueue.Count);

			// when we dequeue the events they should come back in the original order
			for(int i = 0; i < numQEvents; i++)
			{
				IQEvent qEvent = eventQueue.DeQueue();
				Assertion.AssertEquals("Expected signal id", i, qEvent.QSignal);
				Assertion.AssertEquals("Number of events in the queue", 5 - i - 1, eventQueue.Count);
			}
		}

		[Test]
		public void TestBlockingOnEmptyQueue()
		{
			IQEventQueue eventQueue = new QEventQueue();
			EventLoop eventLoop = new EventLoop(eventQueue);
			
			int numQEvents = 3;
			QEvent[] qEvents = new QEvent[numQEvents];
			for(int i = 0; i < numQEvents; i++)
			{
				qEvents[i] = new QEvent(i);
				eventQueue.EnqueueFIFO(qEvents[i]);
				Thread.Sleep(10); // We give the event loop a chance to execute
			}

			eventQueue.EnqueueFIFO(new QEvent(-1)); // We send a 'stop' signal
			eventLoop.Join(); // Wait until the event loop is done

			QEvent[] handledEvents = eventLoop.HandledEvents;

			// we check that the events were handled in the correct order
			for(int i = 0; i < numQEvents; i++)
			{
				Assertion.AssertEquals("Expected QEvent", qEvents[i], handledEvents[i]);
			}
		}

		#region Helper class EventLoop

		private class EventLoop
		{
			private ArrayList m_HandledEvents;
			private IQEventQueue m_EventQueue;
			private Thread m_WorkerThread;

			internal EventLoop(IQEventQueue eventQueue)
			{
				m_HandledEvents = new ArrayList();
				m_EventQueue = eventQueue;
				m_WorkerThread = new Thread(new ThreadStart(this.HandleEvents));
				m_WorkerThread.Start();
			}

			internal void HandleEvents()
			{
				while(true)
				{
					lock(m_HandledEvents)
					{
						IQEvent qEvent = m_EventQueue.DeQueue();
						if (qEvent.QSignal == -1)
						{
							// we use a signal value of -1 to indicate that the loop should stop
							break;
						}
						else
						{
							m_HandledEvents.Add(qEvent);
						}
					}
				}
			}

			/// <summary>
			/// Blocks the calling thread until the loop exited the event loop and ended its thread
			/// </summary>
			internal void Join()
			{
				m_WorkerThread.Join();
			}

			internal QEvent[] HandledEvents
			{
				get
				{
					lock(m_HandledEvents)
					{
						return (QEvent[])m_HandledEvents.ToArray(typeof(QEvent));
					}
				}
			}

			#endregion
		}
	}
}
