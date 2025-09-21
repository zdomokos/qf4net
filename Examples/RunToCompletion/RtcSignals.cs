using System;
using qf4net;

namespace RunToCompletionHsm
{
    public class RtcSignals : QSignals
    {
        public static readonly Signal Start = new Signal("Start");
        public static readonly Signal Abort = new Signal("Abort");
        public static readonly Signal Quit = new Signal("Quit");
    } //RtcSignals
} //namespace RunToCompletionHsm
