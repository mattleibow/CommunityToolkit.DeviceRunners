﻿using Android.App;
using Android.Content;
using Android.Provider;
using Android.OS;
using Android.Runtime;

namespace Xunit.Runner.Devices.XHarness;

public abstract class XHarnessInstrumentationBase : Instrumentation
{
	readonly TaskCompletionSource<Application> _waitForApplication = new();

	protected XHarnessInstrumentationBase(IntPtr handle, JniHandleOwnership ownership)
		: base(handle, ownership)
	{
	}

	protected Bundle? Arguments { get; private set; }

	public override void OnCreate(Bundle? arguments)
	{
		Arguments = arguments;

		base.OnCreate(arguments);

		Start();
	}

	public override void CallApplicationOnCreate(Application? app)
	{
		base.CallApplicationOnCreate(app);

		if (app is null)
			_waitForApplication.SetException(new ArgumentNullException(nameof(app)));
		else
			_waitForApplication.SetResult(app);
	}

	public override async void OnStart()
	{
		base.OnStart();

		var app = await _waitForApplication.Task;

		OnApplicationStart(app);
	}

	public async void OnApplicationStart(Application app)
	{
		var tcs = new TaskCompletionSource<Bundle>();

		var vm = GetHomeViewModel(app);
		vm.TestRunCompleted += OnTestRunCompleted;

		var activity = StartTestActivity(app);

		var bundle = await tcs.Task;

		CopyFile(bundle);

		// activity.Finish();

		Finish(Result.Ok, bundle);

		void OnTestRunCompleted(object? sender, object result) =>
			tcs.SetResult((Bundle)result);
	}

	protected abstract HomeViewModel GetHomeViewModel(Application app);

	protected virtual Activity StartTestActivity(Application app)
	{
		var pm = Context!.PackageManager!;
		var intent = pm.GetLaunchIntentForPackage(Context.PackageName!);
		return StartActivitySync(intent)!;
	}

	void CopyFile(Bundle bundle)
	{
		var resultsFile = bundle.GetString("test-results-path");
		if (resultsFile == null)
			return;

		var guid = Guid.NewGuid().ToString("N");
		var name = Path.GetFileName(resultsFile);

		string finalPath;
		if (!OperatingSystem.IsAndroidVersionAtLeast(30))
		{
			var root = Application.Context.GetExternalFilesDir(null)!.AbsolutePath!;
			var dir = Path.Combine(root, guid);

			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);

			finalPath = Path.Combine(dir, name);
			File.Copy(resultsFile, finalPath, true);
		}
		else
		{
			var downloads = Android.OS.Environment.DirectoryDownloads!;
			var relative = Path.Combine(downloads, Context!.PackageName!, guid);

			var values = new ContentValues();
			values.Put(MediaStore.IMediaColumns.DisplayName, name);
			values.Put(MediaStore.IMediaColumns.MimeType, "text/xml");
			values.Put(MediaStore.IMediaColumns.RelativePath, relative);

			var resolver = Context!.ContentResolver!;
			var uri = resolver.Insert(MediaStore.Downloads.ExternalContentUri, values)!;
			using (var dest = resolver.OpenOutputStream(uri)!)
			using (var source = File.OpenRead(resultsFile))
				source.CopyTo(dest);

#pragma warning disable CS0618 // Type or member is obsolete
			var root = Android.OS.Environment.ExternalStorageDirectory!.AbsolutePath;
#pragma warning restore CS0618 // Type or member is obsolete
			finalPath = Path.Combine(root, relative, name);
		}

		bundle.PutString("test-results-path", finalPath);
	}
}
