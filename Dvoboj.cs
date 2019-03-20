using System;
using Android.App;
using System.IO;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Bluetooth;
using System.Threading;
using Android.Views.Animations;
using Android.Animation;
using System.Threading.Tasks;
using System.Collections.Generic;
using Android.Content.Res;
using Android.Util;

using System.Linq;



namespace WorldOnPalm
{
    [Activity(Label = "WorldOnPalm", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class Dvoboj : Activity
    {

        BluetoothConnection myConnection = new BluetoothConnection();
        BluetoothSocket _socket = null;
        System.Threading.Thread listenThread;

        bool connected = false;

        int x;
        int y;
        int brojBodova=10;

        int igracNaPotezu = 0;
        bool prekid = false;

        int segmentIgre = 0;
        bool igraUTijeku = false;
        bool prikazivanjePitanja = false;
        bool cekanjeOdgovora = false;

        string odgovor;
        int regijaPitanja;
        bool igracOstajeNaPotezu = false;

        bool odgovorIgrac1=false;
        bool odgovorIgrac2=false;

        bool prikazivanjeRezultata = false;

        int prviTocanOdgovor = 0;

        int bodoviIgrac1 = 0;
        int bodoviIgrac2 = 0;


        Dictionary<string, string[]> pitanjaNaPolju = new Dictionary<string, string[]>();



        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (igraUTijeku) return;
            RequestWindowFeature(WindowFeatures.NoTitle);
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            base.OnCreate(savedInstanceState);

            //Dohvaæanje pitanja
            Stream content = Assets.Open("Pitanja.txt");
            StreamReader cont = new StreamReader(content);
            string s = cont.ReadToEnd();
            int k = 0;
            string[] pitanja = s.Split('\n');
            int br = 0;


            for (int i = 97; i < 123; i++)
            {
                pitanjaNaPolju[((char)i).ToString()] = new string[1000];
                for (int j = 0; j < 1000; j++)
                {
                    pitanjaNaPolju[((char)i).ToString()][j] = "";
                }
            }

            for (int i = 0; i < pitanja.Length; i++)
            {
                if (pitanja[i].Length == 0) continue;
                string slovo = pitanja[i][0].ToString();
                string pitanje = pitanja[i].Substring(2);

                for (int j = 0; j < 1000; j++)
                {
                    if (pitanjaNaPolju[slovo][j] == "") { pitanjaNaPolju[slovo][j] = pitanje; break; }
                }
            }


            SetContentView(Resource.Layout.dvobojSpajanjeBT);

            BluetoothSocket _socket = null;

            listenThread = new System.Threading.Thread(listener);
            listenThread.Abort();

            ImageButton btButton = FindViewById<ImageButton>(Resource.Id.btSpajanje);
            ImageButton btButtonNastavi = FindViewById<ImageButton>(Resource.Id.btNastavi);

            try
            {
                disconnect();
            }
            catch { }

            btButton.Click += delegate {

                try
                {
                    connect();
                    if (connected == true)
                    {
                        btButton.SetImageResource(Resource.Drawable.dvobojBTTop2);
                        btButtonNastavi.SetImageResource(Resource.Drawable.nastavi2);
                    }

                }
                catch (Exception ex)
                {
                    //TextView textError = FindViewById<TextView>(Resource.Id.textView1);
                    //textError.Visibility = ViewStates.Visible;
                }

            };

            btButtonNastavi.Click += delegate
            {
                if (connected == true)
                {
                    SetContentView(Resource.Layout.brojIgracaDvoboj);
                    brojIgracaDvoboj();
                }
            };

        }

        void brojIgracaDvoboj()
        {
            ImageButton nastaviButton = FindViewById<ImageButton>(Resource.Id.igraciNastavi);
            ImageButton minusButton = FindViewById<ImageButton>(Resource.Id.igraciMinus);
            ImageButton plusButton = FindViewById<ImageButton>(Resource.Id.igraciPlus);
            TextView brojIgracaPrikaz = FindViewById<TextView>(Resource.Id.brojIgraca);

            minusButton.Click += delegate
            {
                if (brojBodova > 5)
                {
                    brojBodova -= 5;
                    brojIgracaPrikaz.Text = brojBodova.ToString();
                }
            };
            plusButton.Click += delegate
            {
                if (brojBodova < 50)
                {
                    brojBodova += 5;
                    brojIgracaPrikaz.Text = brojBodova.ToString();
                }
            };

            nastaviButton.Click += delegate {

                SetContentView(Resource.Layout.posljednjaUputaDvoboj);
                ImageButton pokreniIgru = FindViewById<ImageButton>(Resource.Id.pokreniIgru);
                pokreniIgru.Click += delegate
                {
                    klasicnaIgraKontrola();
                };

            };

        }

        void connect()
        {

            myConnection = new BluetoothConnection();
            myConnection.getAdapter();
            myConnection.thisAdapter.StartDiscovery();

            try
            {

                myConnection.getDevice();
                myConnection.thisDevice.SetPairingConfirmation(false);
                myConnection.thisDevice.SetPairingConfirmation(true);
                myConnection.thisDevice.CreateBond();


            }
            catch (Exception deviceEX)
            {
                return;
            }

            myConnection.thisAdapter.CancelDiscovery();


            _socket = myConnection.thisDevice.CreateRfcommSocketToServiceRecord(Java.Util.UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));

            myConnection.thisSocket = _socket;

            try
            {
                myConnection.thisSocket.Connect();

                if (listenThread.IsAlive == false)
                {
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        listener();

                    }).Start();
                }
                Console.WriteLine("Uspjesno spajanje");
                connected = true;

            }
            catch (Exception CloseEX)
            {
                return;
            }



        }


        void disconnect()
        {
            try
            {
                listenThread.Abort();

                myConnection.thisDevice.Dispose();

                myConnection.thisSocket.OutputStream.WriteByte(187);
                myConnection.thisSocket.OutputStream.Close();

                myConnection.thisSocket.Close();

                myConnection = new BluetoothConnection();
                _socket = null;
                bool prekid = true;

            }
            catch { }
        }

        void listener()
        {
            byte[] read = new byte[8];

            TextView readTextView = FindViewById<TextView>(Resource.Id.textView2);

            TextView timeTextView = FindViewById<TextView>(Resource.Id.textView3);
            char zadnjeSlovo = 'z';
            while (true)
            {
                if (prekid) { prekid = false; break; }

                try
                {
                    DateTime vrijemeB = DateTime.Now;
                    DateTime vrijemeA;
                    myConnection.thisSocket.InputStream.Read(read, 0, 8);
                    myConnection.thisSocket.InputStream.Close();
                    RunOnUiThread(() =>
                    {
                        char[] letter = System.Text.Encoding.UTF8.GetString(read).ToCharArray();
                        for (int i = 0; i < letter.Length; i++)
                        {
                            vrijemeA = DateTime.Now;
                            TimeSpan razlika = vrijemeA - vrijemeB;
                            if (96 < letter[i] && letter[i] < 123)
                            {
                               if (zadnjeSlovo == letter[i] && razlika.Seconds < 1)
                                {
                                    vrijemeB = DateTime.Now;
                                    continue;
                                }
                                zadnjeSlovo = letter[i];
                                vrijemeB = DateTime.Now;
                                if (prikazivanjePitanja)
                                {
                                    continue;
                                }
                                if (igraUTijeku)
                                {
                                    Console.WriteLine(letter[i].ToString());
                                    pitanjeIliOdgovor(letter[i].ToString());
                                }
                                else
                                {
                                    Console.WriteLine(letter[i]);
                                }
                            }
                        }
                        connected = true;

                    });
                }
                catch (Exception ex)
                {

                    Console.WriteLine(ex.ToString());
                    connected = false;


                }

            }
        }



        void klasicnaIgraKontrola()
        {

            Console.WriteLine("Poèetak igre...");

            igraUTijeku = true;
            segmentIgre = 1;
            igracNaPotezu = 1;

            cekanjePitanja();

        }

        void cekanjePitanja()
        {

            SetContentView(Resource.Layout.Dvoboj1);
            TextView cekanje1 = FindViewById<TextView>(Resource.Id.cekanje1);
            TextView cekanje2 = FindViewById<TextView>(Resource.Id.cekanje2);

            AlphaAnimation animation = new AlphaAnimation(0.2f, 1.0f);
            animation.Duration = 2000;
            animation.FillAfter = true;
            animation.RepeatMode = RepeatMode.Reverse;
            animation.RepeatCount = Animation.Infinite;
            cekanje1.StartAnimation(animation);
            cekanje2.StartAnimation(animation);
        }


        bool provjeriOdgovor(string slovo)
        {
            int odg;
            int igrac = 0;
            if (slovo == "h") { odg = 4; igrac = 1; }
            else if (slovo == "i") { odg = 3; igrac = 1; }
            else if (slovo == "j") { odg = 2; igrac = 1; }
            else if (slovo == "k") { odg = 1; igrac = 1; }

            else if (slovo == "t") { odg = 4; igrac = 2; }
            else if (slovo == "u") { odg = 3; igrac = 2; }
            else if (slovo == "v") { odg = 2; igrac = 2; }
            else if (slovo == "w") { odg = 1; igrac = 2; }
            else return false;


                Button odgovor1 = FindViewById<Button>(Resource.Id.odgovor1);
                Button odgovor2 = FindViewById<Button>(Resource.Id.odgovor2);
                Button odgovor3 = FindViewById<Button>(Resource.Id.odgovor3);
                Button odgovor4 = FindViewById<Button>(Resource.Id.odgovor4);


                if (odg == 1)
                {
                    if (odgovor1.Text == odgovor) tocanOdgovor(true, 1,igrac);
                    else if (odgovor.Length == odgovor1.Text.Length + 1)
                    {
                        if (odgovor.Substring(0, odgovor.Length - 1) == odgovor1.Text) tocanOdgovor(true, 1,igrac);
                        else tocanOdgovor(false, 1, igrac);
                    }
                    else tocanOdgovor(false, 1, igrac);
                }
                else if (odg == 2)
                {
                    if (odgovor2.Text == odgovor) tocanOdgovor(true, 2, igrac);
                    else if (odgovor.Length == odgovor2.Text.Length + 1)
                    {
                        if (odgovor.Substring(0, odgovor.Length - 1) == odgovor2.Text) tocanOdgovor(true, 2, igrac);
                        else tocanOdgovor(false, 2, igrac);
                    }
                    else tocanOdgovor(false, 2, igrac);
                }
                else if (odg == 3)
                {
                    if (odgovor3.Text == odgovor) tocanOdgovor(true, 3, igrac);
                    else if (odgovor.Length == odgovor3.Text.Length + 1)
                    {
                        if (odgovor.Substring(0, odgovor.Length - 1) == odgovor3.Text) tocanOdgovor(true, 3, igrac);
                        else tocanOdgovor(false, 3, igrac);
                    }
                    else tocanOdgovor(false, 3, igrac);
                }
            else if (odg == 4)
            {
                if (odgovor4.Text == odgovor) tocanOdgovor(true, 4, igrac);
                else if (odgovor.Length == odgovor4.Text.Length + 1)
                {
                    if (odgovor.Substring(0, odgovor.Length - 1) == odgovor4.Text) tocanOdgovor(true, 4, igrac);
                    else tocanOdgovor(false, 4, igrac);
                }
                else tocanOdgovor(false, 4, igrac);
            }

            return true;

        }

        void tocanOdgovor(bool tocno, int odg, int igrac)
        {
            if (igrac == 1 && odgovorIgrac1) return;
            if (igrac == 2 && odgovorIgrac2) return;
            Console.WriteLine("Provjera......");
            if (igrac == 1) odgovorIgrac1 = true;
            if (igrac == 2) odgovorIgrac2 = true;
            if (odgovorIgrac1==true && odgovorIgrac2 == true)
            {
                cekanjeOdgovora = false;
            }
            if (prviTocanOdgovor == 0)
            {
                if (igrac == 1 && tocno == true) prviTocanOdgovor = 1;
                if (igrac == 2 && tocno == true) prviTocanOdgovor = 2;
            }

            Button odgovor1 = FindViewById<Button>(Resource.Id.odgovor1);
            Button odgovor2 = FindViewById<Button>(Resource.Id.odgovor2);
            Button odgovor3 = FindViewById<Button>(Resource.Id.odgovor3);
            Button odgovor4 = FindViewById<Button>(Resource.Id.odgovor4);
            Button odgovor12 = FindViewById<Button>(Resource.Id.odgovor12);
            Button odgovor22 = FindViewById<Button>(Resource.Id.odgovor22);
            Button odgovor32 = FindViewById<Button>(Resource.Id.odgovor32);
            Button odgovor42 = FindViewById<Button>(Resource.Id.odgovor42);

            Button[] odgovori = { odgovor1, odgovor2, odgovor3, odgovor4, odgovor12, odgovor22, odgovor32, odgovor42 };
            odg--;

            if (tocno)
            {
                odgovori[odg+(igrac-1)*4].SetBackgroundResource(Resource.Drawable.tocno);
            }
            else
            {
                odgovori[odg + (igrac - 1) * 4].SetBackgroundResource(Resource.Drawable.netocno);
            }
            AlphaAnimation animation = new AlphaAnimation(0.0f, 1.0f);
            animation.Duration = 1000;
            animation.StartOffset = 0;
            animation.FillAfter = true;
            odgovori[odg + (igrac - 1) * 4].StartAnimation(animation);

        }


        async void pitanjeIliOdgovor(string slovo)
        {
            //Èekanje na pitanje
            if (segmentIgre == 1)
            {
                segmentIgre = 2;
                prikazivanjePitanja = true;
                prikaziPitanje(slovo);
                await PutTaskDelay(500);
                return;
            }
            //Èekanje na odgovor
            else if (segmentIgre == 2)
            {
              
                if (!cekanjeOdgovora) return;
                if (!provjeriOdgovor(slovo)) return; 
                //Animacija
                if (prviTocanOdgovor != 0 || (odgovorIgrac1==true && odgovorIgrac2==true) )
                {
                    if (!prikazivanjeRezultata)
                    {
                        prikazivanjeRezultata = true;
                        await PutTaskDelay(2000);
                        prikaziRezultate();
                        await PutTaskDelay(3000);
                        prikazivanjeRezultata = false;

                        segmentIgre = 1;
                        cekanjePitanja();
                    }
                }
                return;
            }
        }

        void prikaziRezultate()
        {

            SetContentView(Resource.Layout.Dvoboj3);

            if (prviTocanOdgovor == 1) bodoviIgrac1++;
            if (prviTocanOdgovor == 2) bodoviIgrac2++;

            TextView bodovi1 = FindViewById<TextView>(Resource.Id.bodovi1);
            TextView bodovi2 = FindViewById<TextView>(Resource.Id.bodovi2);

            bodovi1.Text = bodoviIgrac1.ToString();
            bodovi2.Text = bodoviIgrac2.ToString();

            AlphaAnimation animation2 = new AlphaAnimation(0.0f, 1.0f);
            animation2.Duration = 1000;
            animation2.StartOffset = 0;
            animation2.FillAfter = true;

            bodovi1.StartAnimation(animation2);
            bodovi2.StartAnimation(animation2);
        }

        void prikaziPitanje(string slovo)
        {

            SetContentView(Resource.Layout.Dvoboj2);

            LinearLayout l1 = FindViewById<LinearLayout>(Resource.Id.tipPitanja);
            LinearLayout l2 = FindViewById<LinearLayout>(Resource.Id.pitanje1);
            LinearLayout l3 = FindViewById<LinearLayout>(Resource.Id.pitanje2);

            LinearLayout l12 = FindViewById<LinearLayout>(Resource.Id.tipPitanja2);
            LinearLayout l22 = FindViewById<LinearLayout>(Resource.Id.pitanje12);
            LinearLayout l32 = FindViewById<LinearLayout>(Resource.Id.pitanje22);

            Button odgovor1 = FindViewById<Button>(Resource.Id.odgovor1);
            Button odgovor2 = FindViewById<Button>(Resource.Id.odgovor2);
            Button odgovor3 = FindViewById<Button>(Resource.Id.odgovor3);
            Button odgovor4 = FindViewById<Button>(Resource.Id.odgovor4);

            Button odgovor12 = FindViewById<Button>(Resource.Id.odgovor12);
            Button odgovor22 = FindViewById<Button>(Resource.Id.odgovor22);
            Button odgovor32 = FindViewById<Button>(Resource.Id.odgovor32);
            Button odgovor42 = FindViewById<Button>(Resource.Id.odgovor42);

            TextView tekstPitanja1 = FindViewById<TextView>(Resource.Id.tekstPitanja1);
            TextView tekstPitanja2 = FindViewById<TextView>(Resource.Id.tekstPitanja2);

            TextView tekstPitanja12 = FindViewById<TextView>(Resource.Id.tekstPitanja12);
            TextView tekstPitanja22 = FindViewById<TextView>(Resource.Id.tekstPitanja22);

            TextView imeRegije = FindViewById<TextView>(Resource.Id.imeRegijeTekst);
            TextView kategorijaPitanja = FindViewById<TextView>(Resource.Id.kategorijaPitanjaTekst);

            TextView imeRegije2 = FindViewById<TextView>(Resource.Id.imeRegijeTekst2);
            TextView kategorijaPitanja2 = FindViewById<TextView>(Resource.Id.kategorijaPitanjaTekst2);

            l1.Visibility = ViewStates.Visible;
            l2.Visibility = ViewStates.Visible;
            l3.Visibility = ViewStates.Visible;

            l12.Visibility = ViewStates.Visible;
            l22.Visibility = ViewStates.Visible;
            l32.Visibility = ViewStates.Visible;

            //Ubacivanje pitanja
            string[] pitanje = rendomPitanje(slovo);

            imeRegije.Text = tipPitanja(slovo)[0];
            kategorijaPitanja.Text = tipPitanja(slovo)[1];
            imeRegije2.Text = tipPitanja(slovo)[0];
            kategorijaPitanja2.Text = tipPitanja(slovo)[1];

            regijaPitanja = Int32.Parse(tipPitanja(slovo)[2]);

            tekstPitanja1.Text = pitanje[0];
            tekstPitanja2.Text = pitanje[0];
            odgovor1.Text = pitanje[1];
            odgovor2.Text = pitanje[2];
            odgovor3.Text = pitanje[3];
            odgovor4.Text = pitanje[4];

            tekstPitanja12.Text = pitanje[0];
            tekstPitanja22.Text = pitanje[0];
            odgovor12.Text = pitanje[1];
            odgovor22.Text = pitanje[2];
            odgovor32.Text = pitanje[3];
            odgovor42.Text = pitanje[4];

            odgovor = pitanje[5];
            ////////////////////



            l1.Visibility = ViewStates.Visible;
            l2.Visibility = ViewStates.Gone;
            l3.Visibility = ViewStates.Gone;

            l12.Visibility = ViewStates.Visible;
            l22.Visibility = ViewStates.Gone;
            l32.Visibility = ViewStates.Gone;

            AlphaAnimation animation = new AlphaAnimation(1.0f, 0.0f);
            animation.Duration = 1500;
            animation.StartOffset = 0;
            animation.FillAfter = true;
            animation.RepeatCount = 0;

            l1.StartAnimation(animation);
            l12.StartAnimation(animation);
            pricekaj1();
        }

        private string[] rendomPitanje(string slovo)
        {
            int br = 0;
            for (int i = 0; i < 1000; i++)
            {
                if (pitanjaNaPolju[slovo][i] == "") break;
                br++;
            }
            Random rnd = new Random();
            int brojPitanja = rnd.Next(0, br);
            return pitanjaNaPolju[slovo][brojPitanja].Trim('\n').Split('|');
        }

        private string[] tipPitanja(string slovo)
        {

            //Dalmacija 0  Povijest
            //Istra 1      Zanimljivost
            //Slavonija 2  Geografija
            //Gorska 3     Nasumièno
            //Sjeverna 4
            //Središnja 5
            int[] regija = { 0, 1, 2, 3, 4, 5, 2, 4, 0, 1, 5, 3, 1, 3, 5, 2, 0, 4, 2, 1, 0, 4, 3, 5 };
            int[] kategorija = { 0, 1, 2, 1, 3, 0, 3, 2, 1, 0, 3, 0, 2, 1, 1, 0, 3, 1, 1, 3, 2, 0, 2, 2 };
            string[] regije = { "Dalmacija", "Istra i Sj. Primorje", "Slavonija", "Gorska Hrvatska", "Sjeverna Hrvatska", "Središnja Hrvatska" };
            string[] kategorije = { "POVIJEST", "ZANIMLJIVOSTI", "GEOGRAFIJA", "NASUMIÈNO" };
            string[] kategorijaRegija = { regije[regija[(int)slovo[0] - 97]], kategorije[kategorija[(int)slovo[0] - 97]], regija[(int)slovo[0] - 97].ToString(), kategorija[(int)slovo[0] - 97].ToString() };
            return kategorijaRegija;
        }

        async Task PutTaskDelay(int milisec)
        {
            await Task.Delay(milisec);
        }

        private async void pricekaj1()
        {
            await PutTaskDelay(2000);
            LinearLayout l1 = FindViewById<LinearLayout>(Resource.Id.tipPitanja);
            LinearLayout l2 = FindViewById<LinearLayout>(Resource.Id.pitanje1);
            LinearLayout l3 = FindViewById<LinearLayout>(Resource.Id.pitanje2);

            LinearLayout l12 = FindViewById<LinearLayout>(Resource.Id.tipPitanja2);
            LinearLayout l22 = FindViewById<LinearLayout>(Resource.Id.pitanje12);
            LinearLayout l32 = FindViewById<LinearLayout>(Resource.Id.pitanje22);

            l1.Visibility = ViewStates.Gone;
            l2.Visibility = ViewStates.Visible;

            l12.Visibility = ViewStates.Gone;
            l22.Visibility = ViewStates.Visible;

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
            l22.StartAnimation(animation2);

            //Sklanjanje pitanja
            await PutTaskDelay(5000);
            TextView pitanje = FindViewById<TextView>(Resource.Id.tekstPitanja1);
            TextView pitanje2 = FindViewById<TextView>(Resource.Id.tekstPitanja12);
            pitanje.StartAnimation(animation);
            pitanje2.StartAnimation(animation);
            await PutTaskDelay(1000);


            //Pojava odgovora
            l2.Visibility = ViewStates.Gone;
            l3.Visibility = ViewStates.Visible;

            l22.Visibility = ViewStates.Gone;
            l32.Visibility = ViewStates.Visible;

            prikazivanjePitanja = false;

            //Varijable za igru
            odgovorIgrac1 = false;
            odgovorIgrac2 = false;
            prviTocanOdgovor = 0;


            cekanjeOdgovora = true;

            l3.StartAnimation(animation2);
            l32.StartAnimation(animation2);

        }


    }

}

