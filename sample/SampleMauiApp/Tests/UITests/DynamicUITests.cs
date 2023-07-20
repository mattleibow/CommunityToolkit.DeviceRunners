using System.Collections;

using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace SampleMauiApp;

public class DynamicUITests : UITests<ContentPage>
{
	[UIFact]
	public async Task ButtonIsPositionedCorrectlyInLayouts()
	{
		// create a layout with a control
		Button button;
		var grid = new Grid
		{
			Background = Brush.White,
			Children =
			{
				new Grid
				{
					WidthRequest = 50,
					HeightRequest = 50,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					Background = Brush.Green,
					Children =
					{
						(button = new Button
						{
							Text = "Yay!",
							Background = Brush.Red,
							HorizontalOptions = LayoutOptions.Center,
							VerticalOptions = LayoutOptions.Center,
							MaximumWidthRequest = 60,
							HeightRequest = 30,
						})
					},
				}
			},
		};

		// load the control and wait for a layout pass
		await LoadContent(grid);
		await WaitForLayout(button);

		// verify that it is correct
		Assert.Equal(0, button.X, 1.0);
		Assert.Equal(10, button.Y, 1.0);
		Assert.Equal(50, button.Width, 2.0);
		Assert.Equal(30, button.Height, 2.0);
	}

	[UIFact]
	public async Task ButtonAddedToAPageGetsTheCorrectWindow()
	{
		var button = new Button
		{
			Text = "Yay!",
			Background = Brush.Red,
		};

		Assert.Null(button.Window);

		await LoadContent(button);

		Assert.Equal(CurrentPage.Window, button.Window);
	}

#if WINDOWS && INCLUDE_FAILING_TESTS
	[UITheory]
	[InlineData("hello", "HELLO")]
	[InlineData("woRld", "WORLD")]
	public void SimpleTheory(string text, string expected)
	{
		// create the cross-platform control
		var entry = new Entry
		{
			TextTransform = TextTransform.Uppercase,
			Text = "initial text"
		};

		// create the handler and underlying platform view
		var handler = (EntryHandler)entry.ToHandler(MauiContext);

		Assert.Equal("INITIAL TEXT", entry.Text);

		// emulate the user setting some text via the OS keyboard
		// the UpdateText method is just a hack to set the platform value
		var fakeEntry = new Entry { Text = text };
		handler.PlatformView.UpdateText(fakeEntry);

		// check to make sure the cross-platform view is updated with the transform
		Assert.Equal(expected, entry.Text);
	}

	[UITheory]
	[ClassData(typeof(TestDataGenerator))]
	public void ComplexTheory(ComplexData testData)
	{
		// create the cross-platform control
		var entry = new Entry
		{
			TextTransform = TextTransform.Uppercase,
			Text = "initial text"
		};

		// create the handler and underlying platform view
		var handler = (EntryHandler)entry.ToHandler(MauiContext);

		Assert.Equal("INITIAL TEXT", entry.Text);

		// emulate the user setting some text via the OS keyboard
		// the UpdateText method is just a hack to set the platform value
		var fakeEntry = new Entry { Text = testData.Input };
		handler.PlatformView.UpdateText(fakeEntry);

		// check to make sure the cross-platform view is updated with the transform
		Assert.Equal(testData.Expected, entry.Text);
	}
#endif

	public record ComplexData(string Input, string Expected);

	class TestDataGenerator : IEnumerable<object[]>
	{
		readonly List<object[]> _data = new()
		{
			new object[] { new ComplexData("hello", "HELLO") },
			new object[] { new ComplexData("woRld", "WORLD") }
		};

		public IEnumerator<object[]> GetEnumerator() => _data.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	static readonly Rect InitialFrame = new(0, 0, -1, -1);

	async Task WaitForLayout(View view, Rect? initialFrame = default, int timeout = 5000, int interval = 100)
	{
		initialFrame ??= InitialFrame;

		while (timeout > 0)
		{
			if (view.Frame != initialFrame)
				return;

			await Task.Delay(interval);

			timeout -= interval;
		}
	}

	async Task LoadContent(View content)
	{
		var tcs = new TaskCompletionSource();

		content.Loaded += OnLoaded;

		CurrentPage.Content = content;

		await tcs.Task;

		void OnLoaded(object? sender, EventArgs e)
		{
			content.Loaded -= OnLoaded;
			tcs.SetResult();
		}
	}
}
