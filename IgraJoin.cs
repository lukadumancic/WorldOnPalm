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

using Android.Views.Animations;
using System.Threading.Tasks;



namespace WorldOnPalm
{
    [Activity(Label = "WorldOnPalm", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Unspecified, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class IgraJoin : Activity
    {
        string nick;
        int brojSobe;

        bool connected = false;
        bool prekid = false;

        int segmentIgre = 0;
        bool igraUTijeku = false;
        bool prikazivanjePitanja = false;
        bool cekanjeOdgovora = false;

        string odgovor;
        int regijaPitanja;
        bool igracOstajeNaPotezu = false;

        string result = null;
        WebResponse response = null;
        StreamReader reader = null;

        int[] bodoviIgraèa = { 0, 0, 0, 0 };


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Console.WriteLine("Poèetak igre");
            SetContentView(Resource.Layout.KlasicnaIgraLayout1);

            nick = Intent.GetStringExtra("nick") ?? "Data not available";
            brojSobe = Int32.Parse(Intent.GetStringExtra("brojSobe"));

            new Thread(() =>
            {
                //Thread.CurrentThread.IsBackground = true;
                kontrolaIgre();

            }).Start();

        }


        private void pitanjeIliOdgovor()
        {
            //Èekanje na pitanje
            if (segmentIgre == 1)
            {
                Console.WriteLine("Prikazivanje pitanja");
                prikaziPitanje();
                Console.WriteLine("0000000000");
                prikazivanjePitanja = true;
                return;
            }
            //Èekanje na odgovor
            else if (segmentIgre == 2)
            {
                return;
            }
        }

        void prikaziPitanje()
        {
            Console.WriteLine("11111111111");
            SetContentView(Resource.Layout.KlasicnaIgraLayout2);
            Console.WriteLine("22222222222");
            LinearLayout l1 = FindViewById<LinearLayout>(Resource.Id.tipPitanja);
            LinearLayout l2 = FindViewById<LinearLayout>(Resource.Id.pitanje1);
            LinearLayout l3 = FindViewById<LinearLayout>(Resource.Id.pitanje2);

            Button odgovor1 = FindViewById<Button>(Resource.Id.odgovor1);
            Button odgovor2 = FindViewById<Button>(Resource.Id.odgovor2);
            Button odgovor3 = FindViewById<Button>(Resource.Id.odgovor3);
            Button odgovor4 = FindViewById<Button>(Resource.Id.odgovor4);

            TextView tekstPitanja1 = FindViewById<TextView>(Resource.Id.tekstPitanja1);
            TextView tekstPitanja2 = FindViewById<TextView>(Resource.Id.tekstPitanja2);

            TextView imeRegije = FindViewById<TextView>(Resource.Id.imeRegijeTekst);
            TextView kategorijaPitanja = FindViewById<TextView>(Resource.Id.kategorijaPitanjaTekst);
            Console.WriteLine("333333333333");
            l1.Visibility = ViewStates.Visible;
            l2.Visibility = ViewStates.Visible;
            l3.Visibility = ViewStates.Visible;

            //Ubacivanje pitanja
            string[] pitanje = dohvatiPitanje();

            //imeRegije.Text = tipPitanja(slovo)[0];
            //kategorijaPitanja.Text = tipPitanja(slovo)[1];
            //regijaPitanja = Int32.Parse(tipPitanja(slovo)[2]);

            tekstPitanja1.Text = pitanje[0];
            tekstPitanja2.Text = pitanje[0];
            odgovor1.Text = pitanje[1];
            odgovor2.Text = pitanje[2];
            odgovor3.Text = pitanje[3];
            odgovor4.Text = pitanje[4];

            odgovor = pitanje[5];
            ////////////////////



            l1.Visibility = ViewStates.Visible;
            l2.Visibility = ViewStates.Gone;
            l3.Visibility = ViewStates.Gone;

            /* l1.Rotation = 180 + stranaIgraca[igracNaPotezu] * 90;
             l2.Rotation = 180 + stranaIgraca[igracNaPotezu] * 90;
             l3.Rotation = 180 + stranaIgraca[igracNaPotezu] * 90;*/

            AlphaAnimation animation = new AlphaAnimation(1.0f, 0.0f);
            animation.Duration = 1500;
            animation.StartOffset = 0;
            animation.FillAfter = true;
            animation.RepeatCount = 0;
            l1.StartAnimation(animation);
            pricekaj1();
        }

        private async void pricekaj1()
        {
            await PutTaskDelay(2000);
            LinearLayout l1 = FindViewById<LinearLayout>(Resource.Id.tipPitanja);
            LinearLayout l2 = FindViewById<LinearLayout>(Resource.Id.pitanje1);
            LinearLayout l3 = FindViewById<LinearLayout>(Resource.Id.pitanje2);

            l1.Visibility = ViewStates.Gone;
            l2.Visibility = ViewStates.Visible;

            AlphaAnimation animation = new AlphaAnimation(1.0f, 0.0f);
            animation.Duration = 1000;
            animation.StartOffset = 0;
            animation.FillAfter = true;
            animation.RepeatCount = 0;


            //Pojava pitanja
            AlphaAnimation animation2 = new AlphaAnimation(0.0f, 1.0f);
            animation2.Duration = 1000;
            animation2.StartOffset = 0;
            animation2.FillAfter = true;
            l2.StartAnimation(animation2);


            //Sklanjanje pitanja
            await PutTaskDelay(5000);
            TextView pitanje = FindViewById<TextView>(Resource.Id.tekstPitanja1);
            pitanje.StartAnimation(animation);
            await PutTaskDelay(1000);


            //Pojava odgovora
            l2.Visibility = ViewStates.Gone;
            l3.Visibility = ViewStates.Visible;

            prikazivanjePitanja = false;
            cekanjeOdgovora = true;

            l3.StartAnimation(animation2);

        }

        string[] dohvatiPitanje()
        {
            string[] pitanje;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://worldonpalm.ddns.net/index.php?funkcija=provjeriSobu&brojSobe=" + brojSobe.ToString());
            request.Method = "GET";
            response = request.GetResponse();
            reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            result = reader.ReadToEnd();
            pitanje = result.Split('|')[4].Split('|');
            return pitanje;
        }

        async void kontrolaIgre()
        {

            while (true)
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://worldonpalm.ddns.net/index.php?funkcija=faza&brojSobe=" + brojSobe.ToString());
                    request.Method = "GET";
                    response = request.GetResponse();
                    reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    result = reader.ReadToEnd();
                    if (result.Contains('2'))
                    {
                        Console.WriteLine("Novo pitanjeeee...");
                        segmentIgre = 1;
                        pitanjeIliOdgovor();
                        await PutTaskDelay(5000);
                    }
                }
                catch (IOException e) {
                    Console.WriteLine(e);
                }
            }

        }

        async Task PutTaskDelay(int milisec)
        {
            await Task.Delay(milisec);
        }
    }
}