using NUnit.Framework;
using qf4net;
using System.Collections.Concurrent;

namespace qf4net.UnitTests;

[TestFixture]
public class SignalTest
{

    [SetUp]
    public void Setup()
    {
        // Record the initial signal count (from QSignals static initialization)
    }

    #region Basic Functionality Tests

    [Test]
    public void Constructor_ValidName_CreatesSignal()
    {
        // Act
        var signal = new QSignal("UniqueTestSignal");

        // Assert
        // Assert.That(signal.ToString(), Is.EqualTo("UniqueTestSignal"));
    }

    #endregion

    #region String Representation Tests

    [Test]
    public void ToString_ReturnsSignalName()
    {
        // Arrange
        var signal = new QSignal("ToStringTest");

        // Act & Assert
        Assert.That(signal.ToString(),  Contains.Substring("ToStringTest"));
    }

    #endregion

    #region Equality and Comparison Tests

    [Test]
    public void Equals_DifferentSignals_ReturnsFalse()
    {
        // Arrange
        var signal1 = new QSignal("EqualityTest1");
        var signal2 = new QSignal("EqualityTest2");

        // Act & Assert
        Assert.That(signal1.Equals(signal2), Is.False);
        Assert.That(signal1 == signal2, Is.False);
        Assert.That(signal1 != signal2, Is.True);
    }


    [Test]
    public void CompareTo_WorksCorrectly()
    {
        // Arrange
        var signal1 = new QSignal("CompareTest1");
        var signal2 = new QSignal("CompareTest2");

        // Act & Assert
        Assert.That(signal1.CompareTo(signal2), Is.LessThan(0)); // signal1 created first, lower value
        Assert.That(signal2.CompareTo(signal1), Is.GreaterThan(0));
        Assert.That(signal1.CompareTo(signal1), Is.EqualTo(0));
        Assert.That(signal1.CompareTo(null), Is.GreaterThan(0));
    }

    #endregion

}
