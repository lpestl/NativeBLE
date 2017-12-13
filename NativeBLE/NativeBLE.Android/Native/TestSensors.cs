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
using Java.Util;
using NativeBLE.Droid.Native;
using NativeBLE.Droid.Backend;
using NativeBLE.Core;

namespace NativeBLE.Droid.Native
{
    class TestSensors : Java.Lang.Object, IDialogInterfaceOnClickListener
    {
        private Logger logger = new Logger();

        private static int MinLimit = 800;
        private static int MaxLimit = 3500;
        private LinkedList<String> lMovingAverage = new LinkedList<String>();
        private LinkedList<String> lstDynamicMinA = new LinkedList<String>();
        private LinkedList<String> lstDynamicMaxA = new LinkedList<String>();

        private int MOVING_AVERAGE_SIZE = 4;
        private long result = 0;
        private long average = 0;
        private static long[] measurement = new long[3];
        private static long[,] average_measurement = new long[2, 2];
        private long lMin = 0;
        private long lMax = 0;
        private static bool sensorSelector = false;

        private bool bRecordMax = false;
        private bool bRecordMin = false;

        private static int itrTest = 0;
        private AlertDialog alertDialogSensorA;
        private AlertDialog alertDialogSensorB;

        private Activity activity;
        //private SensorDataPage parent;
        SensorViewModel sensorViewModel;

        public TestSensors(Activity _activity, /*SensorDataPage _parent*/SensorViewModel sensorView)
        {
            activity = _activity;
            //parent = _parent;
            sensorViewModel = sensorView;
            logger.TAG = "TestSensors";            
        }

        public void StartTest()
        {
            alertDialogSensorA = new AlertDialog.Builder(activity).Create();
            alertDialogSensorA.SetTitle("Sensor A: Apply the pressure");
            alertDialogSensorB = new AlertDialog.Builder(activity).Create();
            alertDialogSensorB.SetTitle("Maintain the pressure");
            sensorSelector = false;

            alertDialogSensorA.SetButton((int)DialogButtonType.Positive, "OK", this);

            alertDialogSensorA.Show();
        }

        public void OnClick(IDialogInterface dialog, int which)
        {
            alertDialogSensorB.SetMessage("00:4");
            alertDialogSensorB.Show();
            lMovingAverage.Clear();
            result = 0;
            
            sensorViewModel.DebugString = "Moving Average Cleared";
            logger.TraceInformation("Moving Average Cleared");

            var countTimer = new SensorCountDownTimer(4000, 1000, this);
            countTimer.Start();
        }

        private bool TestValidation()
        {
            int success = 0;
            if (average_measurement[0, 0] > MaxLimit)
            {
                success++;
            }
            else
            {
                logger.TraceInformation("Sensor A too weak");
            }

            if (average_measurement[1, 0] < MinLimit)
            {
                success++;
            }
            else
            {
                logger.TraceInformation("Failure to relase sensor A");
            }

            if (average_measurement[0, 1] > MaxLimit)
            {
                success++;
            }
            else
            {
                logger.TraceInformation("Sensor B too weak");
            }
            if (average_measurement[1, 1] < MinLimit)
            {
                success++;
            }
            else
            {
                logger.TraceInformation("Failure to properly release sensor B");
            }

            if (success == 4)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void SetRecord()
        {
            if (lMovingAverage.Count == MOVING_AVERAGE_SIZE - 1)
            {
                logger.TraceInformation("Stack is full");
                if (bRecordMax == true)
                {
                    lMax = average;
                    bRecordMax = false;
                    logger.TraceInformation("Recording max set to false");
                    bRecordMin = false;
                }
                if (bRecordMin == true)
                {
                    lMin = average;
                    bRecordMin = false;
                    logger.TraceInformation("Recording min set to false");
                    bRecordMax = false;
                }
            }

            //Quand le bouton "Enregistrer max dynamique" est pressé
            //if (bRecordDynamicMax == true)
            //{
            //    for (int i = 0; i < lMovingAverage.Count; i++)
            //    {
            //        logger.TraceInformation((lMovingAverage.ElementAt(i)));
            //    }
            //}
        }

        public void CalculateMovingAverage(String dataA, String dataB)
        {
            String data;
            if (sensorSelector != true)
            {
                data = dataA;
            }
            else
            {
                data = dataB;
            }
            if (data != null)
            {
                if (lMovingAverage.Count < MOVING_AVERAGE_SIZE)
                {
                    lMovingAverage.AddFirst(data);
                    result = result + long.Parse(data);
                }
                else
                {
                    result = result + long.Parse(data);
                    result = result - long.Parse(lMovingAverage.Last.Value);
                    average = result / lMovingAverage.Count;

                    lMovingAverage.RemoveLast();
                    lMovingAverage.AddFirst(data);
                }
            }
        }

        public void SetMinLimit(int value)
        {
            MinLimit = value;
        }

        public class SensorCountDownTimer : CountDownTimer
        {
            private TestSensors parent;
            public SensorCountDownTimer(long millisInFuture, long countDownInterval, TestSensors testSensors) : 
                base(millisInFuture, countDownInterval)
            {
                parent = testSensors;
            }

            public override void OnFinish()
            {
                itrTest++;
                switch (itrTest)
                {
                    case 0:
                        parent.alertDialogSensorA.SetTitle("SENSOR A: Apply the pressure");
                        parent.alertDialogSensorB.SetTitle("Keep on applying pressure");
                        parent.alertDialogSensorB.Dismiss();
                        parent.alertDialogSensorA.Show();
                        break;
                    case 1:
                        parent.alertDialogSensorA.SetTitle("SENSOR A: Release the pressure");
                        parent.alertDialogSensorB.SetTitle("Keep on releasing pressure");
                        Arrays.Sort(measurement);
                        average_measurement[0, 0] = measurement[2];
                        parent.sensorViewModel.SensorA_TopResult = average_measurement[0, 0].ToString();
                        parent.alertDialogSensorB.Dismiss();
                        parent.alertDialogSensorA.Show();
                        break;
                    case 2:
                        parent.alertDialogSensorA.SetTitle("SENSOR B: Apply the pressure");
                        sensorSelector = true;
                        parent.alertDialogSensorB.SetTitle("Keep on applying pressure");
                        Arrays.Sort(measurement);
                        average_measurement[1, 0] = measurement[2];
                        parent.sensorViewModel.SensorA_BottomResult = average_measurement[1, 0].ToString();
                        parent.alertDialogSensorB.Dismiss();
                        parent.alertDialogSensorA.Show();
                        break;
                    case 3:
                        parent.alertDialogSensorA.SetTitle("SENSOR B: Release the pressure");
                        parent.alertDialogSensorB.SetTitle("Keep on releasing pressure");
                        Arrays.Sort(measurement);
                        average_measurement[0, 1] = measurement[2];
                        parent.sensorViewModel.SensorB_TopResult = average_measurement[0, 1].ToString();
                        parent.alertDialogSensorB.Dismiss();
                        parent.alertDialogSensorA.Show();
                        break;
                    case 4:
                        itrTest = 0;
                        sensorSelector = false;
                        Arrays.Sort(measurement);
                        average_measurement[1, 1] = measurement[2];
                        parent.sensorViewModel.SensorB_BottomResult = average_measurement[1, 1].ToString();
                        if (parent.TestValidation() == true)
                        {
                            parent.sensorViewModel.VisibleResult = true;
                            parent.sensorViewModel.TextResult = "TEST PASSED!!!";
                            parent.sensorViewModel.ColorResult = Xamarin.Forms.Color.Green;
                        }
                        else
                        {
                            parent.sensorViewModel.VisibleResult = true;
                            parent.sensorViewModel.TextResult = "TEST FAILED";
                            parent.sensorViewModel.ColorResult = Xamarin.Forms.Color.Red;
                        }
                        
                        parent.alertDialogSensorB.Dismiss();
                        parent.alertDialogSensorA.Dismiss();
                        break;
                }
                parent.logger.TraceWarning(itrTest.ToString());

            }

            public override void OnTick(long millisUntilFinished)
            {
                parent.lMovingAverage.Clear();
                long itrClock;
                itrClock = millisUntilFinished / 1000;
                int i = (int)itrClock;
                parent.logger.TraceWarning("i =");
                parent.logger.TraceWarning(i.ToString());
                measurement[i - 1] = parent.average;
                parent.result = 0;
                parent.logger.TraceWarning(parent.average.ToString());

                parent.alertDialogSensorB.SetMessage("00:" + (millisUntilFinished / 1000));
            }
        }
    }
}