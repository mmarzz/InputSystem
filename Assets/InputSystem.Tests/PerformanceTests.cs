using System;
using System.Reflection;
using ISX;
using NUnit.Framework;
using UnityEngine;

public class PerformanceTests
{
    [SetUp]
    public void Setup()
    {
        InputSystem.Save();
        InputSystem.Reset();
    }

    [TearDown]
    public void TearDown()
    {
        InputSystem.Restore();
    }

    // Performing a full state update on 10 devices should take less than 0.01 ms.
    [Test]
    [Category("Performance")]
    public void TODO_CanUpdate10GamepadsInLessThanPointZeroOneMilliseconds()
    {
        const int kNumGamepads = 10;

        var gamepads = new Gamepad[kNumGamepads];
        for (var i = 0; i < kNumGamepads; ++i)
            gamepads[i] = (Gamepad)InputSystem.AddDevice("Gamepad");

        var startTime = Time.realtimeSinceStartup;

        // Generate a full state update for each gamepad.
        for (var i = 0; i < kNumGamepads; ++i)
            InputSystem.QueueStateEvent(gamepads[i], new GamepadState());

        // Now run the update.
        InputSystem.Update();

        var endTime = Time.realtimeSinceStartup;
        var totalTime = endTime - startTime;

        Assert.That(totalTime, Is.LessThan(0.01 / 1000.0));
        Debug.Log(string.Format("{0}: {1}ms", MethodBase.GetCurrentMethod().Name, totalTime * 1000));
    }

    ////TODO: same test but with several actions listening on each gamepad
}
