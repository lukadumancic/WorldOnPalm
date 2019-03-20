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

using Android.Bluetooth;

namespace WorldOnPalm
{
    public class BluetoothConnection
    {

        public void getAdapter() { this.thisAdapter = BluetoothAdapter.DefaultAdapter; }
        public void getDevice() { this.thisDevice = (from bd in this.thisAdapter.BondedDevices where bd.Name == "HC-05" select bd).FirstOrDefault(); }

        public BluetoothAdapter thisAdapter { get; set; }
        public BluetoothDevice thisDevice { get; set; }

        public BluetoothSocket thisSocket { get; set; }



    }
}