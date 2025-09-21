using qf4net;

namespace DiningPhilosophers;

public class DPPSignal
{
    public static readonly Signal Hungry  = new("Hungry   ");
    public static readonly Signal Done    = new("Done	");
    public static readonly Signal Eat     = new("Eat		");
    public static readonly Signal Timeout = new("Timeout	");
};
