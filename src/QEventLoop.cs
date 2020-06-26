using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace qf4net
{
    /// <summary>
    /// The base class for Active Objects.
    /// </summary>
    public class QEventLoop : IQActive
    {
        public event Action ExecutionAborted;

        private readonly IQEventQueue _eventQueue;
        private int _priority;
        private Threading.IThread _executionThread;
        private QHsm _hsm;

        /// <summary>
        /// Initializes a new instance of the <see cref="QEventLoop"/> class. 
        /// </summary>
        public QEventLoop(QHsm hsm)
        {
            _hsm = hsm;
            _eventQueue = EventQueueFactory.GetEventQueue();
        }

        #region IQEventLoop Members

        /// <summary>
        /// Start the <see cref="IQEventLoop"/> object's thread of execution. The caller needs to assign a unique
        /// priority to every <see cref="IQEventLoop"/> object in the system. 
        /// </summary>
        /// <param name="priority">The priority associated with this <see cref="IQEventLoop"/> object.</param>
        // TODO: Are there more flexible ways to handle the priority? Does it really need to be unique in the whole process / system?
        [MethodImpl(MethodImplOptions.Synchronized)]
        public virtual void Start(int priority)
        {
            if (_executionThread != null)
            {
                throw new InvalidOperationException("This active object is already started. The Start method can only be invoked once.");
            }

            // Note: We use the datatype int for the priority since uint is not CLS compliant
            if (priority < 0)
            {
                throw new ArgumentException("The priority of an Active Object cannot be negative.", "priority");
            }
            _priority = priority;
            // TODO: Leverage the priority
            _executionThread = Threading.ThreadFactory.GetThread(0, this.DoEventLoop);
            _executionThread.Start();
        }

        /// <summary>
        /// The priority associated with this <see cref="IQEventLoop"/> object. Once the <see cref="IQEventLoop"/> object
        /// is started the priority is non-negative. For an <see cref="IQEventLoop"/> object that has not yet been started
        /// the value -1 is returned as the priority.
        /// </summary>
        public int Priority { get { return _priority; } }

        /// <summary>
        /// Post the <see paramref="qEvent"/> directly to the <see cref="IQEventLoop"/> object's event queue
        /// using the FIFO (First In First Out) policy. 
        /// </summary>
        /// <param name="qEvent"></param>
        public void PostFifo(IQEvent qEvent)
        {
            _eventQueue.EnqueueFifo(qEvent);
        }

        /// <summary>
        /// Post the <see paramref="qEvent"/> directly to the <see cref="IQEventLoop"/> object's event queue
        /// using the LIFO (Last In First Out) policy. 
        /// </summary>
        /// <param name="qEvent"></param>
        public void PostLifo(IQEvent qEvent)
        {
            _eventQueue.EnqueueLifo(qEvent);
        }

        #endregion

        /// <summary>
        /// This method is executed on the dedicated thread of this <see cref="QEventLoop"/> instance.
        /// </summary>
        private void DoEventLoop()
        {
            _hsm.Init();
            // Once initialized we kick off our event loop
            try
            {
                while (true)
                {
                    IQEvent qEvent = _eventQueue.DeQueue(); // this blocks if there are no events in the queue
                    //Debug.WriteLine(String.Format("Dispatching {0} on thread {1}.", qEvent.ToString(), Thread.CurrentThread.Name));
                    if (qEvent.IsSignal(QSignals.Terminate))
                        break;
                    _hsm.Dispatch(qEvent);
                    // QF.Propagate(qEvent);
                }
            }
            catch (ThreadAbortException)
            {
                // We use the method Thread.Abort() in this.Abort() to exit from the event loop
                Thread.ResetAbort();
                _executionThread = null;
            }

            // The QEventLoop object ends
            OnExecutionAborted();
        }

        /// <summary>
        /// Aborts the execution thread. Nothing happens thereafter!
        /// </summary>
        internal void Abort()
        {
            // QF.Remove(this);
            _executionThread.Abort();
        }

        internal void Join()
        {
            _executionThread.Join();
            _executionThread = null;
        }

        /// <summary>
        /// Allows a deriving class to react to the fact that the execution 
        /// of the active object has been aborted.
        /// </summary>
        private void OnExecutionAborted()
        {
            ExecutionAborted?.Invoke();
        }
    }
}
