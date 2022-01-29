using System;
using System.Timers;
using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Essentials;

namespace NightClock
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity, ISensorEventListener
    {
        private const float MinAlpha = 0.1f;
        private const float MaxAlpha = 0.6f;
        private TextView? _textView;
        private Timer _timer;
        private float _maxLight = 1;
        private float _lastLight = 1;

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
            Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _textView = FindViewById<TextView>(Resource.Id.textView1);
            _textView.Alpha = 0.6f;

            _maxLight = Preferences.Get(nameof(_maxLight), 1f);

            _timer = new Timer(TimerPeriod.TotalMilliseconds);
            _timer.Elapsed += OnTimerTick;
            _timer.Start();
            OnTimerTick(this, null);

            var sensorService = (SensorManager)GetSystemService(Context.SensorService);
            var lightSensor = sensorService.GetDefaultSensor(SensorType.Light);
            sensorService.RegisterListener(this, lightSensor, SensorDelay.Ui);
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

            DeviceDisplay.KeepScreenOn = hasFocus;
        }

        private void OnTimerTick(object sender, ElapsedEventArgs e)
        {
            _textView.Text = DateTime.Now.ToString("HH:mm").TrimStart('0');
            AutoSize();

            _textView.Alpha = MinAlpha + (MaxAlpha - MinAlpha) * (_lastLight / _maxLight);

            var oldValue = Preferences.Get(nameof(_maxLight), 1f);
            if (oldValue < _maxLight)
                Preferences.Set(nameof(_maxLight), _maxLight);
        }

        public void OnAccuracyChanged(Sensor? sensor, SensorStatus accuracy)
        {
        }

        public void OnSensorChanged(SensorEvent? e)
        {
            if (e.Sensor.Type == SensorType.Light)
            {
                _lastLight = e.Values[0];
                if (_lastLight > _maxLight)
                    _maxLight = _lastLight;
            }
        }
    }
}
