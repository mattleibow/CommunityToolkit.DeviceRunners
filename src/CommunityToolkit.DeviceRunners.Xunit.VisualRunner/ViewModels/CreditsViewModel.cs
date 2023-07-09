namespace CommunityToolkit.DeviceRunners.Xunit.VisualRunner;

public class CreditsViewModel : AbstractBaseViewModel
{
	readonly Lazy<string> _creditsHtml = new(() =>
	{
		var type = typeof(CreditsViewModel);
		var assembly = type.Assembly;
		using var stream = assembly.GetManifestResourceStream("CommunityToolkit.DeviceRunners.Xunit.VisualRunner.Assets.credits.html")!;
		using var reader = new StreamReader(stream, leaveOpen: false);
		return reader.ReadToEnd();
	});

	public CreditsViewModel()
	{
		CreditsHtml = _creditsHtml.Value;
	}

	public string CreditsHtml { get; }
}
