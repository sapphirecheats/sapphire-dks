using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SapphireUI
{
    public class SapphireCheckbox : Control
    {
        private bool _isChecked;
        private Color _currentColor = Color.FromArgb(255, 45, 45, 45);
        private Color _targetColor = Color.FromArgb(255, 45, 45, 45);
        private Color _initialColor = Color.FromArgb(255, 45, 45, 45);
        private System.Windows.Forms.Timer _timer;

        public SapphireCheckbox()
        {
            this.Size = new Size(15, 15);
            this._timer = new System.Windows.Forms.Timer();
            this._timer.Interval = 1; // Adjust this value to change the speed of the color transition.
            this._timer.Tick += Timer_Tick;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        public bool IsChecked
        {
            get { return _isChecked; }
            set {
                _isChecked = value;
                _targetColor = _isChecked ? Color.FromArgb(255, 103, 110, 181) : Color.FromArgb(255, 30,30,30);
                _timer.Start();
            }
        }


        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            _isChecked = !_isChecked;
            _targetColor = _isChecked ? Color.FromArgb(255, 103, 110, 181) : Color.FromArgb(255, 30, 30, 30);
            _timer.Start();
        }

        protected override void OnPaint(PaintEventArgs pevent)
        {
            base.OnPaint(pevent);
            Graphics g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Create a path that represents a rounded rectangle.
            var path = new GraphicsPath();
            var rect = this.ClientRectangle;
            int curveSize = 7; // Adjust this value to change the roundness of the corners.

            // Top left corner
            path.AddArc(rect.X, rect.Y, curveSize, curveSize, 180, 90);
            // Top right corner
            path.AddArc(rect.Right - curveSize - 1, rect.Top, curveSize, curveSize, 270, 90);
            // Bottom right corner
            path.AddArc(rect.Right - curveSize - 1, rect.Bottom - curveSize - 1, curveSize, curveSize, 0, 90);
            // Bottom left corner
            path.AddArc(rect.Left, rect.Bottom - curveSize - 1, curveSize, curveSize, 90, 90);

            path.CloseFigure();

            g.FillPath(new SolidBrush(_currentColor), path);
            Color outlineColor = Color.White;

            if (_isChecked)
                outlineColor = Color.FromArgb(Math.Min(_currentColor.R + 30, 255), Math.Min(_currentColor.G + 30, 255), Math.Min(_currentColor.B + 30, 255));
            else
                outlineColor = Color.FromArgb(Math.Min(_currentColor.R + 15, 255), Math.Min(_currentColor.G + 15, 255), Math.Min(_currentColor.B + 15, 255));


            // Create a pen with the outline color and a width of 1.
            using (var pen = new Pen(outlineColor, 1))
            {
                // Draw the outline.
                g.DrawPath(pen, path);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Calculate the new color based on the current color and target color.
            float r = _currentColor.R + ((_targetColor.R - _currentColor.R) * 0.1f);
            float g = _currentColor.G + ((_targetColor.G - _currentColor.G) * 0.1f);
            float b = _currentColor.B + ((_targetColor.B - _currentColor.B) * 0.1f);
            _currentColor = Color.FromArgb((int)r, (int)g, (int)b);

            // Stop the timer if the color has reached the target color.
            if (_currentColor == _targetColor || _currentColor == _initialColor)
            {
                _timer.Stop();
            }

            // Redraw the checkbox.
            this.Invalidate();
        }
    }
}
