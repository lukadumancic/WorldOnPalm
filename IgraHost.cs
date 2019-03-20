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
using System.Net;
using System.Text;


namespace WorldOnPalm
{
    [Activity(Label = "WorldOnPalm", Theme = "@android:style/Theme.Black.NoTitleBar.Fullscreen", ScreenOrientation = Android.Content.PM.ScreenOrientation.Unspecified, ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize | Android.Content.PM.ConfigChanges.KeyboardHidden)]
    public class IgraHost : Activity
    {
        string nick;
        string brojSobe;

        BluetoothConnection myConnection = new BluetoothConnection();
        BluetoothSocket _socket = null;
        System.Threading.Thread listenThread;

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

        int[] bodoviIgraèa = { 0,0,0,0 };


        Dictionary<string, string[]> pitanjaNaPolju = new Dictionary<string, string[]>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.spajanjeNaBT2);

            nick = Intent.GetStringExtra("nick") ?? "Data not available";
            brojSobe = Intent.GetStringExtra("brojSobe") ?? "Data not available";

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
                    Console.WriteLine("Uspjesno spajanje");
                    pokreniIgru();
                }
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

        void pokreniIgru()
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://worldonpalm.ddns.net/index.php?funkcija=pokreniIgru&brojSobe=" + brojSobe.ToString());
            request.Method = "GET";
            response = request.GetResponse();
            igraUTijeku = true;

        }



    }
}