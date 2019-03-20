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

using System.Net;
using System.IO;

using System.Threading.Tasks;

namespace WorldOnPalm
{
    [Activity(Label = "WorldOnPalm", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class Join : Activity
    {

        string nick;
        int brojSobe;

        string result = null;
        WebResponse response = null;
        StreamReader reader = null;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            nick = Intent.GetStringExtra("nick") ?? "Data not available";

            SetContentView(Resource.Layout.brojSobe);

            ImageButton provjeriSobu = FindViewById<ImageButton>(Resource.Id.provjeriSobu);
            EditText brojSobeText = FindViewById<EditText>(Resource.Id.brojSobeText);


            provjeriSobu.Click += delegate {
                brojSobe = Int32.Parse(brojSobeText.Text);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://worldonpalm.ddns.net/index.php?funkcija=provjeriSobu&brojSobe=" + brojSobe.ToString());
                request.Method = "GET";
                response = request.GetResponse();
                reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                result = reader.ReadToEnd();
                Console.WriteLine(result);
                if (result.Contains("False"))
                {
                    Console.WriteLine("Ne postoji soba");
                }
                else
                {
                    if (imaMjesta())
                    {
                        SetContentView(Resource.Layout.cekanjeAdmina);

                        new Thread(() =>
                        {
                            Thread.CurrentThread.IsBackground = true;
                            cekajAdmina();

                        }).Start();
                    }
                }

            };

        }

        private async void cekajAdmina()
        {
            

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
                    Console.WriteLine(result);
                    if (l[4]=="START" && l[5] == "1")
                    {

                        var activity2 = new Intent(this, typeof(IgraJoin));
                        activity2.PutExtra("nick", nick);
                        activity2.PutExtra("brojSobe", brojSobe.ToString());
                        StartActivity(activity2);
                        break;

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

        bool imaMjesta()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://worldonpalm.ddns.net/index.php?funkcija=provjeriSobu&brojSobe=" + brojSobe.ToString());
            request.Method = "GET";
            response = request.GetResponse();
            reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            result = reader.ReadToEnd();

            string[] l=result.Split('|');

            for(int i = 0; i < l.Length; i++)
            {
                if (l[i] == "")
                {
                    ubaciSe(i);
                    return true;
                }
            }
            return false;

        }

        void ubaciSe(int indeks)
        {
            indeks++;
            Console.WriteLine("http://worldonpalm.ddns.net/index.php?funkcija=noviIgrac&brojSobe=" + brojSobe.ToString() + "&imeIgraca=" + nick + "&brojIgraca=" + indeks.ToString());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://worldonpalm.ddns.net/index.php?funkcija=noviIgrac&brojSobe=" + brojSobe.ToString() + "&imeIgraca=" + nick + "&brojIgraca=" + indeks.ToString());
            request.Method = "GET";
            response = request.GetResponse();

        }
    }
}