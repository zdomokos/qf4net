using NUnit.Framework;
using qf4net;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests;

[TestFixture]
public class SignalTest
{
    private int _initialSignalCount;

    [SetUp]
    public void Setup()
    {
        // Record the initial signal count (from QSignals static initialization)
        _initialSignalCount = Signal.Count;
    }

    #region Basic Functionality Tests

    [Test]
    public void Constructor_ValidName_CreatesSignal()
    {
        // Act
        var signal = new Signal("TestSignal");

        // Assert
        Assert.That(signal.Name, Is.EqualTo("TestSignal"));
        Assert.That(signal.Value, Is.GreaterThan(0));
        Assert.That(signal.ToString(), Is.EqualTo("TestSignal"));
    }

    [Test]
    public void Constructor_NullName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Signal(null));
    }

    [Test]
    public void Constructor_EmptyName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Signal(""));
    }

    [Test]
    public void Constructor_WhitespaceName_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Signal("   "));
    }

    #endregion

    #region Duplicate Prevention Tests

    [Test]
    public void Constructor_DuplicateName_ThrowsInvalidOperationException()
    {
        // Arrange
        var signal1 = new Signal("DuplicateTest");

        // Act & Assert
        var ex = Assert.Throws<InvalidOperationException>(() => new Signal("DuplicateTest"));
        Assert.That(ex.Message, Contains.Substring("already registered"));
        Assert.That(ex.Message, Contains.Substring("DuplicateTest"));
    }

    [Test]
    public void Constructor_CaseSensitive_AllowsSimilarNames()
    {
        // Act
        var signal1 = new Signal("CaseTest");
        var signal2 = new Signal("CASETEST");
        var signal3 = new Signal("casetest");

        // Assert
        Assert.That(signal1.Name, Is.EqualTo("CaseTest"));
        Assert.That(signal2.Name, Is.EqualTo("CASETEST"));
        Assert.That(signal3.Name, Is.EqualTo("casetest"));
        Assert.That(signal1, Is.Not.EqualTo(signal2));
        Assert.That(signal2, Is.Not.EqualTo(signal3));
    }

    #endregion

    #region Name Tracking Tests

    [Test]
    public void GetByName_ExistingSignal_ReturnsCorrectSignal()
    {
        // Arrange
        var originalSignal = new Signal("GetByNameTest");

        // Act
        var retrievedSignal = Signal.GetByName("GetByNameTest");

        // Assert
        Assert.That(retrievedSignal, Is.SameAs(originalSignal));
        Assert.That(retrievedSignal.Name, Is.EqualTo("GetByNameTest"));
    }

    [Test]
    public void GetByName_NonExistingSignal_ReturnsNull()
    {
        // Act
        var result = Signal.GetByName("NonExistentSignal");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetByName_NullName_ReturnsNull()
    {
        // Act & Assert
        Assert.That(Signal.GetByName(null), Is.Null);
        Assert.That(Signal.GetByName(""), Is.Null);
        Assert.That(Signal.GetByName("   "), Is.Null);
    }

    [Test]
    public void Exists_ExistingSignal_ReturnsTrue()
    {
        // Arrange
        var signal = new Signal("ExistsTest");

        // Act & Assert
        Assert.That(Signal.Exists("ExistsTest"), Is.True);
    }

    [Test]
    public void Exists_NonExistingSignal_ReturnsFalse()
    {
        // Act & Assert
        Assert.That(Signal.Exists("NonExistentSignal"), Is.False);
    }

    [Test]
    public void Exists_NullOrEmptyName_ReturnsFalse()
    {
        // Act & Assert
        Assert.That(Signal.Exists(null), Is.False);
        Assert.That(Signal.Exists(""), Is.False);
        Assert.That(Signal.Exists("   "), Is.False);
    }

    #endregion

    #region Collection Properties Tests

    [Test]
    public void RegisteredNames_ContainsAllSignalNames()
    {
        // Arrange
        var testSignal1 = new Signal("CollectionTest1");
        var testSignal2 = new Signal("CollectionTest2");

        // Act
        var registeredNames = Signal.RegisteredNames;

        // Assert
        Assert.That(registeredNames, Contains.Item("CollectionTest1"));
        Assert.That(registeredNames, Contains.Item("CollectionTest2"));
        Assert.That(registeredNames.Count, Is.GreaterThan(_initialSignalCount));
    }

    [Test]
    public void RegisteredSignals_ContainsAllSignals()
    {
        // Arrange
        var testSignal1 = new Signal("RegisteredTest1");
        var testSignal2 = new Signal("RegisteredTest2");

        // Act
        var registeredSignals = Signal.RegisteredSignals;

        // Assert
        Assert.That(registeredSignals, Contains.Item(testSignal1));
        Assert.That(registeredSignals, Contains.Item(testSignal2));
        Assert.That(registeredSignals.Count, Is.GreaterThan(_initialSignalCount));
    }

    [Test]
    public void Count_ReflectsActualSignalCount()
    {
        // Arrange
        var initialCount = Signal.Count;

        // Act
        var signal1 = new Signal("CountTest1");
        var signal2 = new Signal("CountTest2");

        // Assert
        Assert.That(Signal.Count, Is.EqualTo(initialCount + 2));
    }

    #endregion

    #region Thread Safety Tests

    [Test]
    public void ConcurrentSignalCreation_NoDuplicatesCreated()
    {
        // Arrange
        var tasks = new Task[100];
        var createdSignals = new ConcurrentBag<Signal>();
        var exceptions = new ConcurrentBag<Exception>();

        // Act
        for (int i = 0; i < 100; i++)
        {
            var index = i;
            tasks[i] = Task.Run(() =>
            {
                try
                {
                    // Try to create signals with the same name - only one should succeed
                    var signal = new Signal($"ConcurrentTest_{index % 10}");
                    createdSignals.Add(signal);
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            });
        }

        Task.WaitAll(tasks);

        // Assert
        Assert.That(createdSignals.Count, Is.EqualTo(10)); // Only 10 unique names
        Assert.That(exceptions.Count, Is.EqualTo(90)); // 90 duplicate attempts

        // Verify all exceptions are InvalidOperationException for duplicates
        Assert.That(exceptions.All(e => e is InvalidOperationException), Is.True);

        // Verify all created signals have unique names
        var uniqueNames = createdSignals.Select(s => s.Name).Distinct().ToList();
        Assert.That(uniqueNames.Count, Is.EqualTo(10));
    }

    [Test]
    public void ConcurrentGetByName_ThreadSafe()
    {
        // Arrange
        var testSignal = new Signal("ThreadSafeTest");
        var tasks = new Task<Signal>[100];

        // Act
        for (int i = 0; i < 100; i++)
        {
            tasks[i] = Task.Run(() => Signal.GetByName("ThreadSafeTest"));
        }

        Task.WaitAll(tasks);

        // Assert
        Assert.That(tasks.All(t => t.Result == testSignal), Is.True);
    }

    #endregion

    #region String Representation Tests

    [Test]
    public void ToString_ReturnsSignalName()
    {
        // Arrange
        var signal = new Signal("ToStringTest");

        // Act & Assert
        Assert.That(signal.ToString(), Is.EqualTo("ToStringTest"));
    }

    [Test]
    public void ToDetailedString_ReturnsDetailedFormat()
    {
        // Arrange
        var signal = new Signal("DetailedTest");

        // Act
        var detailed = signal.ToDetailedString();

        // Assert
        Assert.That(detailed, Contains.Substring("DetailedTest"));
        Assert.That(detailed, Contains.Substring(":"));
        Assert.That(detailed, Contains.Substring("/"));
        Assert.That(detailed, Contains.Substring(signal.Value.ToString()));
    }

    #endregion

    #region Equality and Comparison Tests

    [Test]
    public void Equals_SameSignal_ReturnsTrue()
    {
        // Arrange
        var signal1 = new Signal("EqualityTest");
        var signal2 = Signal.GetByName("EqualityTest");

        // Act & Assert
        Assert.That(signal1.Equals(signal2), Is.True);
        Assert.That(signal1 == signal2, Is.True);
        Assert.That(signal1 != signal2, Is.False);
    }

    [Test]
    public void Equals_DifferentSignals_ReturnsFalse()
    {
        // Arrange
        var signal1 = new Signal("EqualityTest1");
        var signal2 = new Signal("EqualityTest2");

        // Act & Assert
        Assert.That(signal1.Equals(signal2), Is.False);
        Assert.That(signal1 == signal2, Is.False);
        Assert.That(signal1 != signal2, Is.True);
    }

    [Test]
    public void GetHashCode_ConsistentForSameSignal()
    {
        // Arrange
        var signal = new Signal("HashTest");

        // Act & Assert
        Assert.That(signal.GetHashCode(), Is.EqualTo(signal.GetHashCode()));
        Assert.That(signal.GetHashCode(), Is.EqualTo(signal.Value));
    }

    [Test]
    public void CompareTo_WorksCorrectly()
    {
        // Arrange
        var signal1 = new Signal("CompareTest1");
        var signal2 = new Signal("CompareTest2");

        // Act & Assert
        Assert.That(signal1.CompareTo(signal2), Is.LessThan(0)); // signal1 created first, lower value
        Assert.That(signal2.CompareTo(signal1), Is.GreaterThan(0));
        Assert.That(signal1.CompareTo(signal1), Is.EqualTo(0));
        Assert.That(signal1.CompareTo(null), Is.GreaterThan(0));
    }

    #endregion

    #region Integration Tests with QSignals

    [Test]
    public void QSignals_AllHaveUniqueNames()
    {
        // Act - QSignals should already be initialized statically
        var qSignalNames = new[]
        {
            "Empty", "Init", "Entry", "Exit", "Terminate", "StateJob",
            "Initialized", "Start", "Stop", "Abort", "Pause", "Resume", "Error", "Retry"
        };

        // Assert
        foreach (var name in qSignalNames)
        {
            Assert.That(Signal.Exists(name), Is.True, $"QSignal '{name}' should be registered");
            Assert.That(Signal.GetByName(name), Is.Not.Null, $"QSignal '{name}' should be retrievable");
        }
    }

    #endregion
}