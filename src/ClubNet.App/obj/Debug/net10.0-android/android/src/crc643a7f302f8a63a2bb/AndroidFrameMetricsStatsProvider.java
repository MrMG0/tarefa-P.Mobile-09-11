package crc643a7f302f8a63a2bb;


public class AndroidFrameMetricsStatsProvider
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		android.view.Window.OnFrameMetricsAvailableListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onFrameMetricsAvailable:(Landroid/view/Window;Landroid/view/FrameMetrics;I)V:GetOnFrameMetricsAvailable_Landroid_view_Window_Landroid_view_FrameMetrics_IHandler:Android.Views.Window+IOnFrameMetricsAvailableListenerInvoker, Mono.Android, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null\n" +
			"";
		mono.android.Runtime.register ("Microsoft.Maui.DevFlow.Agent.Profiling.AndroidFrameMetricsStatsProvider, Microsoft.Maui.DevFlow.Agent", AndroidFrameMetricsStatsProvider.class, __md_methods);
	}

	public AndroidFrameMetricsStatsProvider ()
	{
		super ();
		if (getClass () == AndroidFrameMetricsStatsProvider.class) {
			mono.android.TypeManager.Activate ("Microsoft.Maui.DevFlow.Agent.Profiling.AndroidFrameMetricsStatsProvider, Microsoft.Maui.DevFlow.Agent", "", this, new java.lang.Object[] {  });
		}
	}

	public void onFrameMetricsAvailable (android.view.Window p0, android.view.FrameMetrics p1, int p2)
	{
		n_onFrameMetricsAvailable (p0, p1, p2);
	}

	private native void n_onFrameMetricsAvailable (android.view.Window p0, android.view.FrameMetrics p1, int p2);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
