using NUnit.Framework;
using qf4net;
using System;
using System.Collections.Generic;

namespace UnitTests;

[TestFixture]
public class QFsmSimpleTest
{
    [Test]
    public void SimpleTest_CanCreateInstance()
    {
        // Arrange & Act
        var fsm = new SimpleTestFsm();

        // Assert
        Assert.That(fsm, Is.Not.Null);
        Assert.That(fsm.StateMethod, Is.Not.Null);
    }

    [Test]
    public void SimpleTest_CanInit()
    {
        // Arrange
        var fsm = new SimpleTestFsm();

        // Act
        Assert.DoesNotThrow(() => fsm.Init());

        // Assert
        Assert.That(fsm.CurrentStateName, Is.EqualTo("TestState"));
    }

    [Test]
    public void SimpleTest_CanDispatch()
    {
        // Arrange
        var fsm = new SimpleTestFsm();
        fsm.Init();
        var testEvent = new QEvent(QSignals.Entry);

        // Act & Assert
        Assert.DoesNotThrow(() => fsm.Dispatch(testEvent));
    }

    private class SimpleTestFsm : QFsm
    {
        private readonly QState _testState;

        public SimpleTestFsm()
        {
            _testState = TestState;
        }

        protected override void InitializeStateMachine()
        {
            InitializeState(_testState);
        }

        private QState TestState(IQEvent qEvent)
        {
            // Handle all signals, return null to indicate handled
            return null;
        }
    }
}