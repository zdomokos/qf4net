using System;
using qf4net;

namespace OptimizationBreaker
{
    /// <summary>
    /// </summary>
    public class QHsmBase3 : QHsm
    {
        #region Boiler plate static stuff

        protected static new TransitionChainStore s_TransitionChainStore = new TransitionChainStore(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
        );

        static QHsmBase3()
        {
            s_TransitionChainStore.ShrinkToActualSize();
        }

        #endregion

        protected QState m_s0;
        protected QState m_s01;
        protected QState m_s02;

        /// <summary>
        /// Default constructor - initializes all fields to default values
        /// </summary>
        public QHsmBase3()
        {
            m_s0 = new QState(this.s0);
            m_s01 = new QState(this.s01);
            m_s02 = new QState(this.s02);
        }

        /// <summary>
        /// Getter for an optional <see cref="TransitionChainStore"/> that can hold cached
        /// <see cref="TransitionChain"/> objects that are used to optimize static transitions.
        /// </summary>
        protected override TransitionChainStore TransChainStore
        {
            get { return s_TransitionChainStore; }
        }

        /// <summary>
        /// Is called inside of the function Init to give the deriving class a chance to
        /// initialize the state machine.
        /// </summary>
        protected override void InitializeStateMachine()
        {
            Console.Write("top-INIT;");
            InitializeState(m_s0); // initial transition
        }

        protected QState s0(IQEvent qEvent)
        {
            switch (qEvent.QSignal)
            {
                case (int)QSignals.Entry:
                    Console.Write("s0-ENTRY;");
                    return null;
                case (int)QSignals.Exit:
                    Console.Write("s0-EXIT;");
                    return null;
                case (int)QSignals.Init:
                    Console.Write("s0-INIT;");
                    InitializeState(m_s01);
                    return null;
            }
            return this.TopState;
        }

        private static int s_TranIdx_s01_s02 = s_TransitionChainStore.GetOpenSlot();

        protected QState s01(IQEvent qEvent)
        {
            switch (qEvent.QSignal)
            {
                case (int)QSignals.Entry:
                    Console.Write("s01-ENTRY;");
                    return null;
                case (int)QSignals.Exit:
                    Console.Write("s01-EXIT;");
                    return null;
                case (int)MyQSignals.Sig1:
                    Console.Write("s01-Sig1;");
                    TransitionTo(m_s02, s_TranIdx_s01_s02);
                    return null;
            }
            return m_s0;
        }

        private static int s_TranIdx_s02_s01 = s_TransitionChainStore.GetOpenSlot();

        protected virtual QState s02(IQEvent qEvent)
        {
            switch (qEvent.QSignal)
            {
                case (int)QSignals.Entry:
                    Console.Write("s02-ENTRY;");
                    return null;
                case (int)QSignals.Exit:
                    Console.Write("s02-EXIT;");
                    return null;
                case (int)MyQSignals.Sig2:
                    Console.Write("s02-Sig2;");
                    TransitionTo(m_s01, s_TranIdx_s02_s01);
                    return null;
            }
            return m_s0;
        }
    }
}
