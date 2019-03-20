using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace WorldOnPalm
{
    [Activity(Label = "WorldOnPalm", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class HostOrJoin : Activity
    {
        string nick;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            nick = Intent.GetStringExtra("nick") ?? "Data not available";

            SetContentView(Resource.Layout.HostOrJoin);

            ImageButton join = FindViewById<ImageButton>(Resource.Id.pridruziSeButton);
            ImageButton host = FindViewById<ImageButton>(Resource.Id.napraviButton);

            join.Click += delegate
            {

                var activity2 = new Intent(this, typeof(Join));
                activity2.PutExtra("nick", nick);
                StartActivity(activity2);

            };

            host.Click += delegate
            {
                var activity2 = new Intent(this, typeof(Host));
                activity2.PutExtra("nick", nick);
                StartActivity(activity2);


            };

        }
    }
}