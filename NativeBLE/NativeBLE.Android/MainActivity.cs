﻿using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;
using System.Threading.Tasks;

namespace NativeBLE.Droid
{
    [Activity(Label = "NativeBLE", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new Core.App());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            var checkSensors = Native.NativeSensorData.GetInstance();
            if (checkSensors != null)
            {
                checkSensors.OnDestroy();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            var checkSensors = Native.NativeSensorData.GetInstance();
            if (checkSensors != null)
            {
                checkSensors.OnPause();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();
            var checkSensors = Native.NativeSensorData.GetInstance();
            if (checkSensors != null)
            {
                checkSensors.OnResume();
            }
        }
    }
}

