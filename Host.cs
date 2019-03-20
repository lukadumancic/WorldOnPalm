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
using System.Threading;
using Android.Widget;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;


namespace WorldOnPalm
{
    [Activity(Label = "WorldOnPalm", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class Host : Activity
    {
        string nick;

        string result = null;
        WebResponse response = null;
        StreamReader reader = null;

        int brojSobe = new Random().Next(1, 999999);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            nick = Intent.GetStringExtra("nick") ?? "Data not available";

            SetContentView(Resource.Layout.kreiranjeSobe);

            ImageButton napraviSobu = FindViewById<ImageButton>(Resource.Id.napraviSobu);

            napraviSobu.Click += delegate {

                var activity2 = new Intent(this, typeof(IgraHost));
                activity2.PutExtra("nick", nick);
                activity2.PutExtra("brojSobe", brojSobe);
                StartActivity(activity2);


            };

            novaSoba();
        }

        void novaSoba()
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://worldonpalm.ddns.net/index.php?funkcija=novaSoba&brojSobe=" + brojSobe.ToString()+"&imeIgraca="+nick);
            request.Method = "GET";
            response = request.GetResponse();
            reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            result = reader.ReadToEnd();
            if (result.Contains("True"))
            {

                TextView tekstSobe = FindViewById<TextView>(Resource.Id.tekstSobe);
                tekstSobe.Text = nick + "ova soba /" + brojSobe.ToString();

                new Thread(() =>
                {
                    //Thread.CurrentThread.IsBackground = true;
                    urediSobu();

                }).Start();
            }
            
            else
            {
                Console.Write("Greška");
            }

        }

        private async void urediSobu()
        {
            
                Button igrac1Tekst = FindViewById<Button>(Resource.Id.igrac1Tekst);
                Button igrac2Tekst = FindViewById<Button>(Resource.Id.igrac2Tekst);
                Button igrac3Tekst = FindViewById<Button>(Resource.Id.igrac3Tekst);

                while (true)
                {
                    try
                    {
                    
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://worldonpalm.ddns.net/index.php?funkcija=provjeriSobu&brojSobe=" + brojSobe.ToString());
                        request.Method = "GET";
                        response = request.GetResponse();
                        reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                        result = reader.ReadToEnd();
                        string[] l = result.Split('|');
                        if (l[1] != "")
                        {
                            igrac1Tekst.Text = l[1];
                        }
                        if (l[2] != "")
                        {
                            igrac2Tekst.Text = l[2];
                        }
                        if (l[3] != "")
                        {
                            igrac3Tekst.Text = l[3];
                        }

                        await PutTaskDelay(2000);
                    }
                    catch
                    {

                    }

            }
            

        }


        async Task PutTaskDelay(int milisec)
        {
            await Task.Delay(milisec);
        }
    }
}