using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SapphireUI
{
    public class SapphireGroupBoxTops : Control
    {
        private Color _currentColor = Color.FromArgb(255, 45, 45, 45);
        private Color _targetColor = Color.FromArgb(255, 45, 45, 45);
        private Timer _timer;

        public SapphireGroupBoxTops()
        {
            Size = new Size(50, 4);
            _timer = new Timer { Interval = 1 };
            _timer.Tick += (s, e) => {
                _currentColor = Color.FromArgb(
                    (int)(_currentColor.R + (_targetColor.R - _currentColor.R) * 0.1),
                    (int)(_currentColor.G + (_targetColor.G - _currentColor.G) * 0.1),
                    (int)(_currentColor.B + (_targetColor.B - _currentColor.B) * 0.1)
                );
                Invalidate();
            };
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        public Color TargetColor
        {
            get => _targetColor;
            set { _targetColor = value; _timer.Start(); }
        }

        public Color CurrentColor
        {
            get => _currentColor;
            set => _currentColor = value;
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            GraphicsPath path = new GraphicsPath();
            Rectangle rect = ClientRectangle;
            int curveSize = 7;

            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            path.AddArc(rect.Right - curveSize - 1, rect.Top, curveSize, curveSize, 270, 90);
            path.CloseFigure();

            g.FillPath(new SolidBrush(_currentColor), path);

            Color outlineColor = Color.FromArgb(Math.Min(_currentColor.R + 15, 255), Math.Min(_currentColor.G + 15, 255), Math.Min(_currentColor.B + 15, 255));
            using (var pen = new Pen(outlineColor, 1))
            {
                g.DrawPath(pen, path);
            }
        }
    }
}
