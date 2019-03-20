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

using System;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

namespace WorldOnPalm
{
    [Activity(Label = "Room")]
    public class Room : Activity
    {

        string nick;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Room);

            

            novaBrzaIgra();

        }

        private bool novaBrzaIgra()
        {
            Random rnd = new Random();
            int brojSobe = rnd.Next(0, 999999);
            HttpWebRequest rqst = (HttpWebRequest)WebRequest.Create(string.Format("http://82.132.7.168/wop.php?funkcija=novaBrza&nick={0}&brojSobe={1}", nick, brojSobe));
            rqst.Method = "POST";
            rqst.ContentType = "text/xml";
            rqst.ContentLength = 0;
            rqst.Timeout = 3000;

            HttpWebResponse rspns = (HttpWebResponse)rqst.GetResponse();
            Console.WriteLine(rspns);
            if (rspns.ToString() == "True") return true;
            return false;
        }
    }
}