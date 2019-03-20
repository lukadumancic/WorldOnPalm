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
    public class GameTypeMenu : Activity
    {
        ImageView vrstaIgreView;
        string nick;
        int igra = 1;

        int firstTouch;
        int lastTouch;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            RequestWindowFeature(WindowFeatures.NoTitle);

            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.GameTypeManu);

            nick = Intent.GetStringExtra("nick") ?? "Data not available";

            ImageButton pokreniButton = FindViewById<ImageButton>(Resource.Id.pokreniButton);

            pokreniButton.Click += delegate {
                Intent activity2;
                if (igra == 1)
                {
                   activity2 = new Intent(this, typeof(KlasicnaIgra));
                   StartActivity(activity2);
                }
                if (igra == 2)
                {
                    activity2 = new Intent(this, typeof(HostOrJoin));
                    activity2.PutExtra("nick", nick);
                    StartActivity(activity2);
                }
                if (igra == 3)
                {
                    activity2 = new Intent(this, typeof(Dvoboj));
                    StartActivity(activity2);
                }

            };

            vrstaIgreView = FindViewById<ImageView>(Resource.Id.vrstaIgreView);

            vrstaIgreView.Touch += TouchMeImageViewOnTouch;

        }

        void desno()
        {
            if (igra == 1) {
                vrstaIgreView.SetImageResource(Resource.Drawable.vlak);
                igra = 2;
            }
            else if (igra == 2)
            {
                vrstaIgreView.SetImageResource(Resource.Drawable.dvoboj);
                igra = 3;
            }
        }

        void lijevo()
        {
            if (igra == 2)
            {
                vrstaIgreView.SetImageResource(Resource.Drawable.klasicna);
                igra = 1;
            }
            else if (igra == 3)
            {
                vrstaIgreView.SetImageResource(Resource.Drawable.vlak);
                igra = 2;
            }

        }

        private void TouchMeImageViewOnTouch(object sender, View.TouchEventArgs touchEventArgs)
        {

            switch (touchEventArgs.Event.Action)
            {
                case MotionEventActions.Down:
                    firstTouch=(int)touchEventArgs.Event.GetX();
                    break;
                case MotionEventActions.Move:
                    break;

                case MotionEventActions.Up:
                    lastTouch = (int)touchEventArgs.Event.GetX();
                    if(lastTouch - firstTouch < -100)
                    {
                        desno();
                    }
                    else if(lastTouch - firstTouch > 100)
                    {
                        lijevo();
                    }
                    break;

                default:
                    break;
            }
        }
    }
}