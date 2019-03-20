package md5451de887bba1cacaef7c825eb42dd4ea;


public class Nick
	extends android.app.Activity
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"";
		mono.android.Runtime.register ("WorldOnPalm.Nick, WorldOnPalm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", Nick.class, __md_methods);
	}


	public Nick () throws java.lang.Throwable
	{
		super ();
		if (getClass () == Nick.class)
			mono.android.TypeManager.Activate ("WorldOnPalm.Nick, WorldOnPalm, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "", this, new java.lang.Object[] {  });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);

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
