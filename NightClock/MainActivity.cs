using System;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;

namespace NightClock
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISensorEventListener
    {
        private const int MaxLightLum = 40000;
        private const float MinAlpha = 0.05f;
        private const float MaxAlpha = 0.5f;
        private TextView? _textView;
        private Timer _timer;

        private static readonly StatusBarVisibility FullscreenFlags = (StatusBarVisibility)(
            SystemUiFlags.HideNavigation |
            SystemUiFlags.LayoutHideNavigation |
            SystemUiFlags.LayoutFullscreen |
            SystemUiFlags.Fullscreen |
            SystemUiFlags.LayoutStable |
            SystemUiFlags.ImmersiveSticky);

        private static readonly TimeSpan TimerPeriod = TimeSpan.FromSeconds(5);

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _textView = FindViewById<TextView>(Resource.Id.textView1);
            _textView.Alpha = 0.6f;

            _timer = new Timer(TimerPeriod.TotalMilliseconds);
            _timer.Elapsed += OnTimerTick;
            _timer.Start();
            OnTimerTick(this, null);

            var sensorService = (SensorManager)GetSystemService(Context.SensorService);
            var lightSensor = sensorService.GetDefaultSensor(SensorType.Light);
            sensorService.RegisterListener(this, lightSensor, SensorDelay.Normal);
        }

        private void AutoSize()
        {
            var textW = _textView.MeasuredWidth;
            var textH = _textView.MeasuredHeight;

            if (textW == 0 || textH == 0)
                return;

            var screenW = Resources.DisplayMetrics.WidthPixels;
            var screenH = Resources.DisplayMetrics.HeightPixels;

            var rW = (float)screenW / textW;
            var rH = (float)screenH / textH;
            var r = 0.9f * Math.Min(rW, rH);
            _textView.ScaleX = r;
            _textView.ScaleY = r;
        }

        public override bool OnTouchEvent(MotionEvent? e)
        {
            if (e.Action == MotionEventActions.Down)
            {
                return true;

                _textView.Alpha += 0.5f;
                while (_textView.Alpha > 1)
                    _textView.Alpha--;
            }

            return base.OnTouchEvent(e);
        }

        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);

            if (hasFocus)
                Window.DecorView.SystemUiVisibility = FullscreenFlags;
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            _textView.Text = DateTime.Now.ToString("hh:mm").TrimStart('0');
            AutoSize();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.menu_main, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var id = item.ItemId;
            if (id == Resource.Id.action_settings)
            {
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent? e)
        {
            if (e.Sensor.Type == SensorType.Light)
            {
                var light = e.Values[0];
                var r = light / MaxLightLum;
                _textView.Alpha = MinAlpha + (MaxAlpha - MinAlpha) * r;
            }
        }
    }
}
