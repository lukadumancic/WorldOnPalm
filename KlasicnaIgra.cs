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
using Android.Content;
using Android.Content.Res;
using Android.Util;

using System.Linq;



namespace WorldOnPalm
{
    [Activity(Label = "WorldOnPalm", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Unspecified, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class KlasicnaIgra : Activity
    {

        BluetoothConnection myConnection = new BluetoothConnection();
        BluetoothSocket _socket = null;
        System.Threading.Thread listenThread;

        bool connected=false;
        int brojIgraca = 3;
        bool biranjeStrana = false;

        int stranaGore = 0;
        int stranaDolje = 0;
        int stranaDesno = 0;
        int stranaLijevo = 0;

        int x;
        int y;

        int[] stranaIgraca=new int[6];

        int igracNaPotezu = 0;
        bool prekid = false;

        int segmentIgre = 0;
        bool igraUTijeku = false;
        bool prikazivanjePitanja = false;
        bool cekanjeOdgovora = false;

        string odgovor;
        int regijaPitanja;
        bool igracOstajeNaPotezu = false;
        

        int[,] bodoviIgraèa = { { 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0, 0, 0 } };


        Dictionary<string, string[]> pitanjaNaPolju = new Dictionary<string, string[]>();



        protected override void OnCreate(Bundle savedInstanceState)
        {
            if (igraUTijeku) return;
            RequestWindowFeature(WindowFeatures.NoTitle);
            this.Window.SetFlags(WindowManagerFlags.KeepScreenOn, WindowManagerFlags.KeepScreenOn);

            base.OnCreate(savedInstanceState);

            //Dohvaæanje pitanja
            Stream content=Assets.Open("Pitanja.txt");
            StreamReader cont=new StreamReader(content);
            string s=cont.ReadToEnd();
            int k = 0;
            string[] pitanja = s.Split('\n');
            int br = 0;

                
            for(int i = 97; i < 123; i++)
            {
                pitanjaNaPolju[((char)i).ToString()] = new string[1000];
                for(int j = 0; j < 1000; j++)
                {
                    pitanjaNaPolju[((char)i).ToString()][j] = "";
                }
            }

            for(int i = 0; i < pitanja.Length; i++)
            {
                if (pitanja[i].Length == 0) continue;
                string slovo = pitanja[i][0].ToString();
                string pitanje = pitanja[i].Substring(2);

                for(int j = 0; j < 1000; j++)
                {
                    if (pitanjaNaPolju[slovo][j] == "") { pitanjaNaPolju[slovo][j] = pitanje; break; }
                }
            }


            SetContentView(Resource.Layout.spajanjeBT);

            BluetoothSocket _socket = null;

            listenThread = new System.Threading.Thread(listener);
            listenThread.Abort();

            ImageButton btButton = FindViewById<ImageButton>(Resource.Id.btSpajanje);
            ImageButton btButtonNastavi = FindViewById<ImageButton>(Resource.Id.btNastavi);

            try
            {
                disconnect();
            }
            catch {}

            btButton.Click += delegate {
                
                try
                {
                    connect();
                    if (connected == true)
                    {
                        btButton.SetImageResource(Resource.Drawable.btSpojeno);
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
                    SetContentView(Resource.Layout.brojIgraca);
                    brojIgracaLayout();
                }
            };

        }

        void brojIgracaLayout()
        {
            ImageButton minusButton = FindViewById<ImageButton>(Resource.Id.igraciMinus);
            ImageButton plusButton = FindViewById<ImageButton>(Resource.Id.igraciPlus);
            TextView brojIgracaPrikaz = FindViewById<TextView>(Resource.Id.brojIgraca);
            ImageButton nastaviButton = FindViewById<ImageButton>(Resource.Id.igraciNastavi);

            minusButton.Click += delegate
            {
                if (brojIgraca > 2)
                {
                    brojIgraca -= 1;
                    brojIgracaPrikaz.Text = brojIgraca.ToString();
                }
            };
            plusButton.Click += delegate
            {
                if (brojIgraca < 4)
                {
                    brojIgraca += 1;
                    brojIgracaPrikaz.Text = brojIgraca.ToString();
                }
            };
            nastaviButton.Click += delegate {

                SetContentView(Resource.Layout.odabirStrana);
                biranjeStrana = true;
                igracNaPotezu = 1;
                odabirStranaLayout();

            };

        }

        void odabirStranaLayout(string slovo="asdf")
        {
            TextView gore = FindViewById<TextView>(Resource.Id.stranaGore);
            TextView dolje = FindViewById<TextView>(Resource.Id.stranaDolje);
            TextView desno = FindViewById<TextView>(Resource.Id.stranaDesno);
            TextView lijevo = FindViewById<TextView>(Resource.Id.stranaLijevo);

            ImageButton nastavi = FindViewById<ImageButton>(Resource.Id.brojNastavi);

            TextView uputa1 = FindViewById<TextView>(Resource.Id.uputa1);
            TextView uputa2 = FindViewById<TextView>(Resource.Id.uputa2);

            if (slovo == "asdf") return;
            int s = strana(slovo);
            if (s == 1 && stranaGore != 0 || s == 2 && stranaDesno != 0 || s == 3 && stranaDolje != 0 || s == 4 && stranaLijevo != 0) { uputa2.Text = "Strana veæ odabrana"; return; }
            Console.WriteLine(stranaDesno);
            Console.WriteLine(s);
            if (s == 1)
            {
                stranaGore = igracNaPotezu;
                stranaIgraca[igracNaPotezu]= 1;
                gore.Text = "Igraè " + igracNaPotezu.ToString();
            }
            else if (s == 2)
            {
                stranaDesno = igracNaPotezu;
                stranaIgraca[igracNaPotezu]= 2;
                desno.Text = "Igraè " + igracNaPotezu.ToString();
            }
            else if (s == 3)
            {
                stranaDolje = igracNaPotezu;
                stranaIgraca[igracNaPotezu] = 3;
                dolje.Text = "Igraè " + igracNaPotezu.ToString();
            }
            else if (s == 4)
            {
                stranaLijevo = igracNaPotezu;
                stranaIgraca[igracNaPotezu] = 4;
                lijevo.Text = "Igraè " + igracNaPotezu.ToString();
            }
            igracNaPotezu++;

            uputa2.Text = "";
            
            if (igracNaPotezu > brojIgraca)
            {
                biranjeStrana = false;
                nastavi.SetImageResource(Resource.Drawable.nastavi2);
                uputa1.Text = "Strane odabrane...";
                nastavi.Click += delegate {
                    SetContentView(Resource.Layout.posljednjaUputa);
                    ImageButton pokreniIgru = FindViewById<ImageButton>(Resource.Id.pokreniIgru);
                    pokreniIgru.Click += delegate
                    {
                        klasicnaIgraKontrola();
                    };
                };
            }
            else {
                uputa1.Text = "Igraè " + igracNaPotezu.ToString() + " bira stranu";
            }
            



        }

        private int strana(string slovo)
        {
            if (slovo[0] < 'g') return 1;
            else if (slovo[0] < 'm') return 2;
            else if (slovo[0] < 't') return 3;
            return 4;

        }
        
    /*async Task btPermission()
        {
            string[] permissions =
            {
                Android.Manifest.Permission.Bluetooth,
                Android.Manifest.Permission.BluetoothAdmin,
                Android.Manifest.Permission.BluetoothPrivileged,
            };

            const int RequestLocationId = 0;
            //Check to see if any permission in our group is available, if one, then all are
            const string permission1 = Android.Manifest.Permission.Bluetooth;
            const string permission2 = Android.Manifest.Permission.BluetoothAdmin;
            const string permission3 = Android.Manifest.Permission.BluetoothPrivileged;
            RequestPermissions(permissions, RequestLocationId);

            
        }*/

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
                Console.WriteLine(deviceEX);
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
                Console.WriteLine(CloseEX);
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
                                if (zadnjeSlovo == letter[i] && razlika.Seconds<1)
                                {
                                    vrijemeB = DateTime.Now;
                                    continue;
                                }
                                zadnjeSlovo = letter[i];
                                vrijemeB = DateTime.Now;
                                if (biranjeStrana)
                                {
                                    odabirStranaLayout(letter[i].ToString());
                                }
                                if (razlika.Seconds < 2 || prikazivanjePitanja)
                                {
                                    continue;
                                }
                                if (igraUTijeku)
                                {
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
                catch (Exception ex) {

                    Console.WriteLine(ex.ToString());
                    connected = false;
                    
                    
                }

            }
        }

        void klasicnaIgraKontrola()
        {
            biranjeStrana = false;

            Console.WriteLine("Poèetak igre...");

            igraUTijeku = true;
            segmentIgre = 1;
            igracNaPotezu = 1;

            cekanjePitanja();

        }

        async void pitanjeIliOdgovor(string slovo)
        {
            //Èekanje na pitanje
            if (segmentIgre == 1)
            {
                Console.WriteLine("Igra igraè" + igracNaPotezu.ToString());
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
                if(!provjeriOdgovor(slovo))return;
                //Animacija
                await PutTaskDelay(3000);
                prikaziRezultate();
                await PutTaskDelay(6000);

                segmentIgre = 1;

                //Igraè na potezu
                if(!igracOstajeNaPotezu) igracNaPotezu++;
                if (igracNaPotezu > brojIgraca) igracNaPotezu = 1;

                cekanjePitanja();
                return;
            }
        }

        void prikaziPitanje(string slovo)
        {

            if (stranaIgraca[igracNaPotezu] == 1)
            {
                this.RequestedOrientation = Android.Content.PM.ScreenOrientation.ReverseLandscape;
            }
            else if (stranaIgraca[igracNaPotezu] == 3)
            {
                this.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
            }
            else if (stranaIgraca[igracNaPotezu] == 4)
            {
                this.RequestedOrientation = Android.Content.PM.ScreenOrientation.ReversePortrait;
            }
            else
            {
                this.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }

            SetContentView(Resource.Layout.KlasicnaIgraLayout2);

            LinearLayout l1 = FindViewById<LinearLayout>(Resource.Id.tipPitanja);
            LinearLayout l2 = FindViewById<LinearLayout>(Resource.Id.pitanje1);
            LinearLayout l3 = FindViewById<LinearLayout>(Resource.Id.pitanje2);

            Button odgovor1 = FindViewById<Button>(Resource.Id.odgovor1);
            Button odgovor2 = FindViewById<Button>(Resource.Id.odgovor2);
            Button odgovor3 = FindViewById<Button>(Resource.Id.odgovor3);
            Button odgovor4 = FindViewById<Button>(Resource.Id.odgovor4);

            TextView tekstPitanja1= FindViewById<TextView>(Resource.Id.tekstPitanja1);
            TextView tekstPitanja2 = FindViewById<TextView>(Resource.Id.tekstPitanja2);

            TextView imeRegije = FindViewById<TextView>(Resource.Id.imeRegijeTekst);
            TextView kategorijaPitanja = FindViewById<TextView>(Resource.Id.kategorijaPitanjaTekst);

            l1.Visibility = ViewStates.Visible;
            l2.Visibility = ViewStates.Visible;
            l3.Visibility = ViewStates.Visible;

            //Ubacivanje pitanja
            string[] pitanje = rendomPitanje(slovo);

            imeRegije.Text = tipPitanja(slovo)[0];
            kategorijaPitanja.Text = tipPitanja(slovo)[1];
            regijaPitanja = Int32.Parse(tipPitanja(slovo)[2]);

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

        void cekanjePitanja()
        {
            if (stranaIgraca[igracNaPotezu] == 1)
            {
                this.RequestedOrientation = Android.Content.PM.ScreenOrientation.ReverseLandscape;
            }
            else if (stranaIgraca[igracNaPotezu] == 3)
            {
                this.RequestedOrientation = Android.Content.PM.ScreenOrientation.Landscape;
            }
            else if (stranaIgraca[igracNaPotezu] == 4)
            {
                this.RequestedOrientation = Android.Content.PM.ScreenOrientation.ReversePortrait;
            }
            else
            {
                this.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            }

            SetContentView(Resource.Layout.KlasicnaIgraLayout1);
            TextView igracNaPotezuTekst = FindViewById<TextView>(Resource.Id.igracNaPotezuTekst);

            AlphaAnimation animation = new AlphaAnimation(0.2f, 1.0f);
            animation.Duration = 2000;
            animation.FillAfter = true;
            animation.RepeatMode = RepeatMode.Reverse;
            animation.RepeatCount = Animation.Infinite;
            igracNaPotezuTekst.StartAnimation(animation);

            igracNaPotezuTekst.Text = "Igra igraè " + igracNaPotezu.ToString() + " ...";
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

        private string[] rendomPitanje(string slovo)
        {
            int br = 0;
            for(int i = 0; i < 1000; i++)
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
            int[] regija =  { 0, 1, 2, 3, 4, 5,2, 4, 0, 1, 5, 3,1, 3, 5,2, 0, 4, 2, 1,0, 4, 3, 5 };
            int[] kategorija = {0,1,2,1,3,0,3,2,1,0,3,0,2,1,1,0,3,1,1,3,2,0,2,2 };
            string[] regije = { "Dalmacija", "Istra i Sj. Primorje", "Slavonija", "Gorska Hrvatska", "Sjeverna Hrvatska", "Središnja Hrvatska" };
            string[] kategorije = { "POVIJEST", "ZANIMLJIVOSTI", "GEOGRAFIJA", "NASUMIÈNO" };
            string[] kategorijaRegija = { regije[regija[(int)slovo[0] - 97]], kategorije[kategorija[(int)slovo[0] - 97]], regija[(int)slovo[0] - 97].ToString(), kategorija[(int)slovo[0] - 97].ToString() };
            return kategorijaRegija;
        }

        bool provjeriOdgovor(string slovo)
        {
            int odg;
            if (stranaIgraca[igracNaPotezu] == 1)
            {
                if (slovo == "b") odg = 4;
                else if (slovo == "c") odg = 3;
                else if (slovo == "d") odg = 2;
                else if (slovo == "e") odg = 1;
                else return false;
            }
            else if (stranaIgraca[igracNaPotezu] == 2)
            {
                if (slovo == "h") odg = 4;
                else if (slovo == "i") odg = 3;
                else if (slovo == "j") odg = 2;
                else if (slovo == "k") odg = 1;
                else return false;
            }
            else if (stranaIgraca[igracNaPotezu] == 3)
            {
                if (slovo == "n") odg = 4;
                else if (slovo == "o") odg = 3;
                else if (slovo == "p") odg = 2;
                else if (slovo == "q") odg = 1;
                else return false;
            }
            else if (stranaIgraca[igracNaPotezu] == 4)
            {
                if (slovo == "t") odg = 4;
                else if (slovo == "u") odg = 3;
                else if (slovo == "v") odg = 2;
                else if (slovo == "w") odg = 1;
                else return false;
            }
            else return false;


            Button odgovor1 = FindViewById<Button>(Resource.Id.odgovor1);
            Button odgovor2 = FindViewById<Button>(Resource.Id.odgovor2);
            Button odgovor3 = FindViewById<Button>(Resource.Id.odgovor3);
            Button odgovor4 = FindViewById<Button>(Resource.Id.odgovor4);


            if (odg == 1)
            {
                if (odgovor1.Text == odgovor) tocanOdgovor(true,1);
                else if(odgovor.Length== odgovor1.Text.Length + 1)
                {
                    if(odgovor.Substring(0,odgovor.Length-1)== odgovor1.Text) tocanOdgovor(true,1);
                    else tocanOdgovor(false, 1);
                }
                else tocanOdgovor(false, 1);
            }
            else if (odg == 2)
            {
                if (odgovor2.Text == odgovor) tocanOdgovor(true,2);
                else if (odgovor.Length == odgovor2.Text.Length + 1)
                {
                    if (odgovor.Substring(0, odgovor.Length - 1) == odgovor2.Text) tocanOdgovor(true,2);
                    else tocanOdgovor(false, 2);
                }
                else tocanOdgovor(false, 2);
            }
            else if (odg == 3)
            {
                if (odgovor3.Text == odgovor) tocanOdgovor(true,3);
                else if (odgovor.Length == odgovor3.Text.Length + 1)
                {
                    if (odgovor.Substring(0, odgovor.Length - 1) == odgovor3.Text) tocanOdgovor(true,3);
                    else tocanOdgovor(false, 3);
                }
                else tocanOdgovor(false, 3);
            }
            else if (odg == 4)
            {
                if (odgovor4.Text == odgovor) tocanOdgovor(true,4);
                else if (odgovor.Length == odgovor4.Text.Length + 1)
                {
                    if (odgovor.Substring(0, odgovor.Length - 1) == odgovor4.Text) tocanOdgovor(true,4);
                    else tocanOdgovor(false, 4);
                }
                else tocanOdgovor(false, 4);
            }


            return true;

        }

        void tocanOdgovor(bool tocno, int odg)
        {
            cekanjeOdgovora = false;

            Button odgovor1 = FindViewById<Button>(Resource.Id.odgovor1);
            Button odgovor2 = FindViewById<Button>(Resource.Id.odgovor2);
            Button odgovor3 = FindViewById<Button>(Resource.Id.odgovor3);
            Button odgovor4 = FindViewById<Button>(Resource.Id.odgovor4);

            Button[] odgovori = { odgovor1, odgovor2, odgovor3, odgovor4 };
            odg--;
            if (tocno)
            {
                odgovori[odg].SetBackgroundResource(Resource.Drawable.tocno);
                igracOstajeNaPotezu = true;
            }
            else
            {
                odgovori[odg].SetBackgroundResource(Resource.Drawable.netocno);
                igracOstajeNaPotezu = false;
            }
            AlphaAnimation animation = new AlphaAnimation(0.0f, 1.0f);
            animation.Duration = 1000;
            animation.StartOffset = 0;
            animation.FillAfter = true;
            odgovori[odg].StartAnimation(animation);

        }

        async void prikaziRezultate()
        {
            SetContentView(Resource.Layout.Rezultati);

            TextView t= FindViewById<ProgressBar>(Resource.Id.textView1);
            t.Text = "REZULTATI IGRAÈ" + igracNaPotezu.ToString();

            ProgressBar pb1 = FindViewById<ProgressBar>(Resource.Id.progressBar1);
            ProgressBar pb2 = FindViewById<ProgressBar>(Resource.Id.progressBar2);
            ProgressBar pb3 = FindViewById<ProgressBar>(Resource.Id.progressBar3);
            ProgressBar pb4 = FindViewById<ProgressBar>(Resource.Id.progressBar4);
            ProgressBar pb5 = FindViewById<ProgressBar>(Resource.Id.progressBar5);
            ProgressBar pb6 = FindViewById<ProgressBar>(Resource.Id.progressBar6);

            ProgressBar pbV= FindViewById<ProgressBar>(Resource.Id.verticalBar);

            ProgressBar[] pb = { pb1, pb2, pb3, pb4, pb5, pb6};

            for(int i = 0; i < 6; i++)
            {
                pb[i].Progress=10+bodoviIgraèa[igracNaPotezu,i]*30;
            }
            pbV.Progress = 10 + bodoviIgraèa[igracNaPotezu, 6] * 10;

            if (igracOstajeNaPotezu)
            {
                //3 boda u regiji i više
                if (bodoviIgraèa[igracNaPotezu, regijaPitanja] == 3)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        pbV.Progress++;
                        await PutTaskDelay(90);
                    }
                    bodoviIgraèa[igracNaPotezu, 6]++;
                }
                //Manje od 3 boda u regiji
                else
                {
                    for (int i = 0; i < 30; i++)
                    {
                        pb[regijaPitanja].Progress++;
                        await PutTaskDelay(30);
                    }
                    bodoviIgraèa[igracNaPotezu, regijaPitanja]++;
                    if(bodoviIgraèa[igracNaPotezu, regijaPitanja] == 3)
                    {
                        for (int i = 0; i < 30; i++)
                        {
                            pbV.Progress+=3;
                            await PutTaskDelay(60);
                        }
                    }

                }

            }

        }


    }

}

