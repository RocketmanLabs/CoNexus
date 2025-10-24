/* Orchestrator.cs
 *
 * Controls the activation and deactivation of background services that run on timed intervals.
 *
 * Descr:
 * The Orchestrator is instantiated as a singleton service, and runs the lifetime of the app.
 *
 * Data:
 *
 * Changelog:
 *
 *
 */

namespace CoNexus.Api.Services;

public class Orchestrator
{
	private readonly Dictionary<string, OrchestratorRegistration> _dor = [];

	public Orchestrator AddService(OrchestratorRegistration orchReg);
}

public class OrchestratorRegistration(string name, TimeSpan interval, bool rtxhn)
{
	private CancellationToken _controlToken;
	private Action<OrchestratorRegistration> _activator = null!;
	
	protected OrchestratorRegistration(string name, TimeSpan interval, bool rtexh, Action<OrchestratorRegistration> activator)
	{
		Name = name;
		Interval = interval;
		RunToExhaustion = rtexh;
		activator = 
	}

	public string Name { get; set; } = "New Service";
	public TimeSpan Interval { get; set; } = TimeSpan.Zero; // run continuously, else wait for interval to run again
	public bool RunToExhaustion { get; set; }  // runs continuously until no more records are available for a period of time in Interval

	public static Orchestrator Register(string name, TimeSpan interval, bool rtexh, Action<OrchestratorRegistration> activator)
	{
		OrchestratorRegistration reg = new();
		ork.Name = name
	}

	public static Orchestrator Start(string name)
	{

	}

	public static Orchestrator Stop(string name)
	{

	}

	public static Orchestrator StopAll(string name)
	{

	}
}