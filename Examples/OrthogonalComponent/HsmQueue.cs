using System;
using System.Collections;
using qf4net;

namespace OrthogonalComponentHsm
{
	/// <summary>
	/// A lightweight FIFO queue extension to the base HSM.
	/// </summary>
	/// <remarks>
	/// When USE_DOTNET_EVENTS is defined:
	/// Designed to allow a state machine to post NEW messages to itself during the handling of a dispatch call. 
	/// The assumption is that most environments (Windows, XWindows) will support event handling and that the 
	/// only time this mechanism will be required is for self-posting. Therefore, the public interface is limited 
	/// to a single method, DispatchQ. 
	/// Otherwise:
	/// Designed to allow any component to post events to the container's queue. The 
	/// implementation adheres to the pattern's requirements that the component not call
	/// the container's dispatch method directly and that the events posted from component 
	/// to container be done so asynchronously (queued).
	/// </remarks>
	public abstract class QHsmQ : QHsm
	{
		/// <summary>
		/// FIFO event queue
		/// </summary>
		protected Queue q;

		/// <summary>
		/// Constructor for the Quantum Hierarchical State Machine with Queue
		/// </summary>
		public QHsmQ()
		{
			q = new Queue();
		}


		/// <summary>
		/// If USE_DOTNET_EVENTS is defined, this method is intended to be used only for self-posting.
		/// Otherwise, it allows any object to add events to the Hsm's queue.
		/// </summary>
		/// <param name="qEvent">New message posted (to self) during processing</param>
		#if USE_DOTNET_EVENTS
		protected void Enqueue(QEvent qEvent)
		#else
		public void Enqueue(IQEvent qEvent)
		#endif
		{
			q.Enqueue(qEvent);
		}

		/// <summary>
		/// Dequeues and dispatches the queued events to this state machine
		/// </summary>
		protected void DispatchQ()
		{
			if (isDispatching)
			{
				return;
			}

			isDispatching = true;
			while (q.Count > 0)
			{
				//new events may be added (self-posted) during the dispatch handling of this first event
				base.Dispatch((QEvent)q.Dequeue());
			}
			isDispatching = false;

		}//DispatchQ

		protected bool isDispatching = false;

		/// <summary>
		/// Enqueues the first event then dequeues and dispatches all queued events to this state machine.
		/// Designed to be called in place of base.Dispatch in the event self-posting is to be 
		/// supported.
		/// </summary>
		public void DispatchQ(IQEvent qEvent)
		{
			q.Enqueue(qEvent);
			DispatchQ();

		}//DispatchQ

		protected void ClearQ()
		{
			q.Clear();
		}

	}//QHsmQ

}//namespace OrthogonalComponentHsm