﻿using System.Reflection;

namespace DeviceRunners.VisualRunners;

public class VisualTestRunnerConfiguration : IVisualTestRunnerConfiguration
{
	public VisualTestRunnerConfiguration(
		IReadOnlyList<Assembly> testAssemblies,
		IResultChannel? resultChannel = null,
		bool autoStart = false,
		bool autoTerminate = false)
	{
		TestAssemblies = testAssemblies?.ToList() ?? throw new ArgumentNullException(nameof(testAssemblies));
		ResultChannel = resultChannel;
		AutoStart = autoStart;
		AutoTerminate = autoTerminate;
	}

	public IReadOnlyList<Assembly> TestAssemblies { get; }

	public IResultChannel? ResultChannel { get; }

	public bool AutoStart { get; }

	public bool AutoTerminate { get; }
}
