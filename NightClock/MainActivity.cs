using System;
using System.Timers;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Xamarin.Essentials;
using Color = Android.Graphics.Color;

namespace NightClock
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const float MinAlpha = 0.25f;
        private const float MaxAlpha = 0.75f;
        private const float TextSize = 0.9f;
        private TextView? _textView;
        private Timer _timer;
        private int _colorIndex;

        private static readonly Color[] Colors = new[]
        {
            Color.Rgb(0, 255, 0),
            Color.Rgb(128, 255, 0),
            Color.Yellow,
            Color.Orange,
            Color.Red,
            Color.Magenta,
            Color.Blue,
            Color.Aqua,
            Color.White
        };

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
            _textView.Alpha = Preferences.Get(nameof(_textView.Alpha), MaxAlpha);

            _colorIndex = Preferences.Get(nameof(_colorIndex), 0);
            _textView.SetTextColor(Colors[_colorIndex]);

            _timer = new Timer(TimerPeriod.TotalMilliseconds);
            _timer.Elapsed += OnTimerTick;
            _timer.Start();
            OnTimerTick(this, null);
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
            var r = TextSize * Math.Min(rW, rH);
            _textView.ScaleX = r;
            _textView.ScaleY = r;
        }

        public override bool OnTouchEvent(MotionEvent? e)
        {
            if (e.Action == MotionEventActions.Up)
            {
                if (e.RawX > e.RawY)
                    ChangeAlpha();
                else
                    ChangeColor();

                return true;
            }

            return base.OnTouchEvent(e);
        }

        private void ChangeAlpha()
        {
            _textView.Alpha = _textView.Alpha > 0.5f ? MinAlpha : MaxAlpha;
            Preferences.Set(nameof(_textView.Alpha), _textView.Alpha);
        }

        private void ChangeColor()
        {
            _colorIndex++;
            if (_colorIndex >= Colors.Length)
                _colorIndex = 0;
            _textView.SetTextColor(Colors[_colorIndex]);
            Preferences.Set(nameof(_colorIndex), _colorIndex);
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
            _textView.Text = TimeToString(DateTime.Now);
            AutoSize();
        }

        private static string TimeToString(DateTime time)
        {
            return $"{time.Hour}:{time.Minute:00}";
        }
    }
}
