using System;
using qf4net;

namespace DiningPhilosophers
{
    public class DPPSignal
    {
        public static readonly Signal Hungry = new Signal("Hungry   ");
        public static readonly Signal Done = new Signal("Done	");
        public static readonly Signal Eat = new Signal("Eat		");
        public static readonly Signal Timeout = new Signal("Timeout	");
    };
}
