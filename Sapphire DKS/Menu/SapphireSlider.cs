using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SapphireUI
{
    using KeyListener;

    public class SapphireSlider : Control
    {
        private static GlobalKeyListener keyListener = new GlobalKeyListener();
        private decimal _value, _minimum, _maximum = 100m, _targetValue;
        private bool _isFloat, _shouldUpdateTargetValue;
        private Timer _timer;
        private Color _trackColor;
        private string _symbol;

        public decimal Value
        {
            get => IsFloat ? _value : (int)_value;
            set
            {
                _targetValue = Math.Min(Math.Max(value, _minimum), _maximum);
                _shouldUpdateTargetValue = true;
                Invalidate();
            }
        }

        public string Symbol { get => _symbol; set => _symbol = value; }
        public decimal Minimum { get => _minimum; set => _minimum = value; }
        public decimal Maximum { get => _maximum; set => _maximum = value; }
        public bool IsFloat { get => _isFloat; set => _isFloat = value; }
        public Color TrackColor { get => _trackColor; set => _trackColor = value; }

        public SapphireSlider()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.ResizeRedraw | ControlStyles.OptimizedDoubleBuffer, true);
            DoubleBuffered = true;
            _timer = new Timer { Interval = 1 };
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_shouldUpdateTargetValue && _value != _targetValue)
            {
                _value = Math.Abs(_value - _targetValue) < 0.25m ? _targetValue : _value + (_targetValue - _value) * 0.2m;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingQuality = CompositingQuality.HighQuality;

            Rectangle trackRect = ClientRectangle;
            trackRect.Inflate(-4, -3);
            int lineWidth = 8;
            LinearGradientBrush lineBrush = new LinearGradientBrush(trackRect, _trackColor, _trackColor, LinearGradientMode.ForwardDiagonal);
            Pen linePen = new Pen(lineBrush, lineWidth) { EndCap = LineCap.Round, StartCap = LineCap.Round };
            int lineEndPoint = trackRect.Width - 3;
            g.DrawLine(linePen, trackRect.Left, (trackRect.Top + trackRect.Height / 2), lineEndPoint, (trackRect.Top + trackRect.Height / 2));

            LinearGradientBrush lineBrush1 = new LinearGradientBrush(trackRect, Color.FromArgb(255, 124, 132, 219), Color.FromArgb(255, 59, 63, 105), LinearGradientMode.ForwardDiagonal);
            Pen linePen1 = new Pen(lineBrush1, lineWidth) { EndCap = LineCap.Round, StartCap = LineCap.Round };
            int lineEndPoint1 = (int)((_value / _maximum * (trackRect.Width - lineWidth)) + lineWidth / 2) + 1;
            g.DrawLine(linePen1, trackRect.Left, (trackRect.Top + trackRect.Height / 2), lineEndPoint1, (trackRect.Top + trackRect.Height / 2));
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Capture = true;
            _shouldUpdateTargetValue = true;
            UpdateTargetValue(e.X);
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            int increment = keyListener.IsKeyPressed(Keys.LShiftKey) ? 10 : 1;
            _targetValue = Math.Min(Math.Max(_targetValue + (e.Delta > 0 ? increment : -increment), _minimum), _maximum);
            _shouldUpdateTargetValue = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            Capture = false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (Capture && _shouldUpdateTargetValue)
                UpdateTargetValue(e.X);
        }

        private void UpdateTargetValue(int mouseX)
        {
            _targetValue = (decimal)(mouseX / (double)Width * (double)_maximum);
            _targetValue = Math.Min(Math.Max(_targetValue, _minimum), _maximum);
            if (!_isFloat) _targetValue = (int)_targetValue;
        }
    }
}