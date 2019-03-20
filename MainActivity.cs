using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using System.Threading;


using System.Linq;



namespace WorldOnPalm
{

    [Activity(Label = "WorldOnPalm", MainLauncher = true, Icon = "@drawable/icon", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Pocetna);
            //RequestWindowFeature(WindowFeatures.NoTitle);

            ImageButton pocetnaButton = FindViewById<ImageButton>(Resource.Id.pocetnaButton);

            pocetnaButton.Click += delegate {
                StartActivity(typeof(Nick));
            };

        }
    }

}

