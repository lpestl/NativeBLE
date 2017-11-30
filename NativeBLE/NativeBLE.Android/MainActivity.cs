using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android;
using System.Threading.Tasks;
using NativeBLE.Droid.Backend;
using Android.Content;
using NativeBLE.Droid.Native;
using Xamarin.Forms;

namespace NativeBLE.Droid
{
    [Activity(Label = "NativeBLE", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Logger logger = new Logger();

        protected override void OnCreate(Bundle bundle)
        {
            logger.TAG = "MainActivity";
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new Core.App());
        }

        protected override void OnDestroy()
        {
            logger.TraceInformation("MainActivity called OnDestroy and stop service");
            MessagingCenter.Send<MainActivity>(this, "OnDestroy");
            base.OnDestroy();
        }

        protected override void OnPause()
        {
            logger.TraceInformation("MainActivity called OnPause");
            MessagingCenter.Send<MainActivity>(this, "OnPause");
            base.OnPause();
        }

        protected override void OnResume()
        {
            base.OnResume();
            logger.TraceInformation("MainActivity called OnResume");
            MessagingCenter.Send<MainActivity>(this, "OnResume");
        }

        protected override void OnStart()
        {
            base.OnStart();
            logger.TraceInformation("MainActivity called OnStart and BindService");
            MessagingCenter.Send<MainActivity>(this, "OnStart");
        }        
    }
}

