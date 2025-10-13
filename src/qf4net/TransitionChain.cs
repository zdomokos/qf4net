using System.Collections;
using System.Diagnostics;
using System.Reflection;

namespace qf4net;

#region Helper classes for the handling of static transitions


/// <summary>
/// This class is used to record the individual transition steps that are required to transition from
/// a given state to a target state.
/// </summary>
public class TransitionChainRecorder
{
    public void Record(QState stateMethod, QSignal qSignal)
    {
        _transitionSteps.Add(new TransitionStep(stateMethod, qSignal));
    }

    /// <summary>
    /// Returns the recorded transition steps in form of a <see cref="TransitionChain"/> instance.
    /// </summary>
    /// <returns></returns>
    public TransitionChain GetRecordedTransitionChain()
    {
        // We turn the List into a strongly typed array
        return new TransitionChain(_transitionSteps);
    }

    private readonly List<TransitionStep> _transitionSteps = [];
}

#endregion

#region TransitionChain & TransitionStep

/// <summary>
/// Class that wraps the handling of recorded transition steps.
/// </summary>
public class TransitionChain
{
    // holds the transitions that need to be performed from the LCA down to the target state
    private readonly QState[] _stateMethodChain;

    // holds the actions that need to be performed on each transition in two bits:
    // 0x1: Init; 0x2: Entry, 0x3: Exit
    private readonly BitArray _actionBits;

    public TransitionChain(List<TransitionStep> transitionSteps)
    {
        _stateMethodChain = new QState[transitionSteps.Count];
        _actionBits = new BitArray(transitionSteps.Count * 2);

        for (var i = 0; i < transitionSteps.Count; i++)
        {
            var transitionStep = transitionSteps[i];

            _stateMethodChain[i] = transitionStep.StateMethod;
            var bitPos = i * 2;

            if (QSignals.Empty == transitionStep.QSignal)
            {
                _actionBits[bitPos] = false;
                _actionBits[++bitPos] = false;
            }
            else if (QSignals.Init == transitionStep.QSignal)
            {
                _actionBits[bitPos] = false;
                _actionBits[++bitPos] = true;
            }
            else if (QSignals.Entry == transitionStep.QSignal)
            {
                _actionBits[bitPos] = true;
                _actionBits[++bitPos] = false;
            }
            else if (QSignals.Exit == transitionStep.QSignal)
            {
                _actionBits[bitPos] = true;
                _actionBits[++bitPos] = true;
            }
        }
    }

    public int Length => _stateMethodChain.Length;

    public TransitionStep this[int index]
    {
        get
        {
            var transitionStep = new TransitionStep { StateMethod = _stateMethodChain[index] };

            var bitPos = index * 2;
            if (_actionBits[bitPos])
            {
                transitionStep.QSignal = _actionBits[bitPos + 1] ? QSignals.Exit : QSignals.Entry;
            }
            else
            {
                transitionStep.QSignal = _actionBits[bitPos + 1] ? QSignals.Init : QSignals.Empty;
            }

            return transitionStep;
        }
    }
}

public struct TransitionStep
{
    public QState StateMethod;
    public QSignal QSignal;

    public TransitionStep(QState stateMethod, QSignal qSignal)
    {
        StateMethod = stateMethod;
        QSignal = qSignal;
    }
}

#endregion

#region TransitionChainStore

/// <summary>
/// Class that handles storage and access to the various <see cref="TransitionChain"/> instances
/// that are required for all the static transitions in use by a given hierarchical state machine.
/// </summary>
public class TransitionChainStore
{
    private const int CDefaultCapacity = 16;

    private TransitionChain[] _items;

    /// <summary>
    /// Constructs a <see cref="TransitionChainStore"/>. The internal array for holding
    /// <see cref="TransitionChain"/> instances is configured to have room for the static
    /// transitions in the base class (if any).
    /// </summary>
    /// <param name="callingClass">The class that called the constructor.</param>
    public TransitionChainStore(Type callingClass)
    {
        Debug.Assert(IsDerivedFromQHsm(callingClass));

        var baseType = callingClass.BaseType;
        var slotsRequiredByBaseQHsm = 0;

        while (baseType != typeof(QHsm))
        {
            slotsRequiredByBaseQHsm += RetrieveStoreSizeOfBaseClass(baseType);
            baseType = baseType.BaseType;
        }

        InitializeStore(slotsRequiredByBaseQHsm);
    }

    private int RetrieveStoreSizeOfBaseClass(Type baseType)
    {
        var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField;

        var mi = baseType.FindMembers(MemberTypes.Field, bindingFlags, Type.FilterName, "s_TransitionChainStore");

        if (mi.Length < 1)
        {
            return 0;
        }

        var store = (TransitionChainStore)baseType.InvokeMember("s_TransitionChainStore", bindingFlags, null, null, null);
        return store.Size;
    }

    private bool IsDerivedFromQHsm(Type type)
    {
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType == typeof(QHsm))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        // None of the base classes is QHsm
        return false;
    }

    private void InitializeStore(int slotsRequiredByBaseQHsm)
    {
        if (slotsRequiredByBaseQHsm == 0)
        {
            _items = new TransitionChain[CDefaultCapacity];
        }
        else
        {
            _items = new TransitionChain[2 * slotsRequiredByBaseQHsm];
        }

        Size = slotsRequiredByBaseQHsm;
    }

    /// <summary>
    /// Creates a new slot for a <see cref="TransitionChain"/> and returns its index
    /// </summary>
    /// <returns>The index of the new slot.</returns>
    public int GetOpenSlot()
    {
        if (Size >= _items.Length)
        {
            // We no longer have room in the items array to hold a new slot
            IncreaseCapacity();
        }

        return Size++;
    }

    /// <summary>
    /// Reallocates the internal array <see cref="_items"/> to an array twice the previous capacity.
    /// </summary>
    private void IncreaseCapacity()
    {
        int newCapacity;
        if (_items.Length == 0)
        {
            newCapacity = CDefaultCapacity;
        }
        else
        {
            newCapacity = _items.Length * 2;
        }

        var newItems = new TransitionChain[newCapacity];
        Array.Copy(_items, 0, newItems, 0, _items.Length);
        _items = newItems;
    }

    /// <summary>
    /// Should be called once all required slots have been established in order to minimize the memory
    /// footprint of the store.
    /// </summary>
    public void ShrinkToActualSize()
    {
        var newItems = new TransitionChain[Size];
        Array.Copy(_items, 0, newItems, 0, Size);
        _items = newItems;
    }

    /// <summary>
    /// Provides access to the array that holds the persisted <see cref="TransitionChain"/> objects.
    /// </summary>
    public TransitionChain[] TransitionChains => _items;

    /// <summary>
    /// The size of the <see cref="TransitionChainStore"/>; i.e., the actual number of used slots.
    /// </summary>
    public int Size { get; private set; }
}

#endregion
