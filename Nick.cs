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
    public class Nick : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Nick);

            Button nickButton = FindViewById<Button>(Resource.Id.nickButton);
            EditText nickText = FindViewById<EditText>(Resource.Id.nickText);
            nickButton.Click += delegate {
                if(nickText.Text.Length>=4)
                {

                    var activity2 = new Intent(this, typeof(GameTypeMenu));
                    activity2.PutExtra("nick", nickText.Text);
                    StartActivity(activity2);
                }
            };

        }
    }
}