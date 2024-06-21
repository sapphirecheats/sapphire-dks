using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SapphireUI
{
    public class SapphireGroupBoxes : Control
    {
        private Color _currentColor = Color.FromArgb(255, 45, 45, 45);
        private Color _targetColor = Color.FromArgb(255, 45, 45, 45);
        private Color _initialColor = Color.FromArgb(255, 45, 45, 45);
        private System.Windows.Forms.Timer _timer;
        private Point previousLocation = Point.Empty;

        public SapphireGroupBoxes()
        {
            this.Size = new Size(50, 50);
            this._timer = new System.Windows.Forms.Timer();
            this._timer.Interval = 1; // Adjust this value to change the speed of the color transition.
            this._timer.Tick += Timer_Tick;

            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.SupportsTransparentBackColor, true);
        }

        public Color TargetColor
        {
            get { return _targetColor; }
            set {
                _targetColor = value;
                _timer.Start();
            }
        }

        public Color CurrentColor
        {
            get { return _currentColor; }
            set {
                _currentColor = value; }
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

            Color outlineColor = Color.FromArgb(Math.Min(_currentColor.R + 15, 255), Math.Min(_currentColor.G + 15, 255), Math.Min(_currentColor.B + 15, 255));

            // Create a pen with the outline color and a width of 1.
            using (var pen = new Pen(outlineColor, 1))
            {
                // Draw the outline.
                g.DrawPath(pen, path);
            }

            foreach (Control control in this.Controls)
            {
                if (control is Label label)
                {
                    Rectangle labelBounds = label.Bounds;
                    if (label.BackColor != Color.Transparent)
                    {
                        g.FillRectangle(new SolidBrush(_currentColor), labelBounds);
                    }
                    TextRenderer.DrawText(g, label.Text, label.Font, labelBounds, label.ForeColor);
                }
            }

        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            float r = _currentColor.R + ((_targetColor.R - _currentColor.R) * 0.1f);
            float g = _currentColor.G + ((_targetColor.G - _currentColor.G) * 0.1f);
            float b = _currentColor.B + ((_targetColor.B - _currentColor.B) * 0.1f);
            _currentColor = Color.FromArgb((int)r, (int)g, (int)b);

            this.Invalidate();
        }
    }
}
