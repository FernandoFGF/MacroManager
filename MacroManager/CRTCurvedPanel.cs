using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MacroManager
{
    /// <summary>
    /// CRT Curved Screen effect using barrel distortion with vignetting.
    /// Uses a grid-based distortion approach for better performance than pixel-by-pixel processing.
    /// </summary>
    public class CRTScreenOverlay : Control
    {
        private float _distortionStrength = 0.08f;  // Barrel distortion strength
        private float _vignetteStrength = 0.35f;    // Vignetting strength
        private const int GRID_SIZE = 20;           // Size of distortion grid cells

        public CRTScreenOverlay()
        {
            this.SetStyle(ControlStyles.UserPaint, true);
            this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            this.SetStyle(ControlStyles.Opaque, false);
            this.DoubleBuffered = true;
            
            this.BackColor = Color.Transparent;
            this.Dock = DockStyle.Fill;
            this.TabIndex = 9999;
        }

        public float DistortionStrength
        {
            get { return _distortionStrength; }
            set { _distortionStrength = Math.Max(0, Math.Min(1, value)); }
        }

        public float VignetteStrength
        {
            get { return _vignetteStrength; }
            set { _vignetteStrength = Math.Max(0, Math.Min(1, value)); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (Width <= 0 || Height <= 0)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Draw barrel distortion grid
            DrawBarrelDistortion(e.Graphics, Width, Height);
            
            // Draw vignetting overlay
            DrawVignette(e.Graphics, Width, Height);

            base.OnPaint(e);
        }

        private void DrawBarrelDistortion(Graphics g, int width, int height)
        {
            float centerX = width / 2f;
            float centerY = height / 2f;

            using (Pen gridPen = new Pen(Color.FromArgb(40, 100, 200, 100), 1.5f))
            {
                // Draw horizontal lines with barrel distortion
                for (int y = 0; y <= height; y += GRID_SIZE)
                {
                    PointF[] points = new PointF[width / GRID_SIZE + 1];
                    
                    for (int i = 0; i < points.Length; i++)
                    {
                        int x = i * GRID_SIZE;
                        if (x > width) x = width;
                        
                        PointF distorted = ApplyBarrelDistortion(x, y, centerX, centerY, width, height);
                        points[i] = distorted;
                    }
                    
                    if (points.Length > 1)
                        g.DrawLines(gridPen, points);
                }

                // Draw vertical lines with barrel distortion
                for (int x = 0; x <= width; x += GRID_SIZE)
                {
                    PointF[] points = new PointF[height / GRID_SIZE + 1];
                    
                    for (int i = 0; i < points.Length; i++)
                    {
                        int y = i * GRID_SIZE;
                        if (y > height) y = height;
                        
                        PointF distorted = ApplyBarrelDistortion(x, y, centerX, centerY, width, height);
                        points[i] = distorted;
                    }
                    
                    if (points.Length > 1)
                        g.DrawLines(gridPen, points);
                }
            }
        }

        private PointF ApplyBarrelDistortion(float x, float y, float centerX, float centerY, int width, int height)
        {
            // Normalize to -1 to 1 range
            float nx = (x - centerX) / centerX;
            float ny = (y - centerY) / centerY;

            // Calculate distance from center
            float r = (float)Math.Sqrt(nx * nx + ny * ny);

            // Apply barrel distortion formula
            float factor = 1f + (_distortionStrength * r * r);
            
            // Apply distortion
            float distX = centerX + (nx / factor) * centerX;
            float distY = centerY + (ny / factor) * centerY;

            return new PointF(distX, distY);
        }

        private void DrawVignette(Graphics g, int width, int height)
        {
            float centerX = width / 2f;
            float centerY = height / 2f;
            float maxRadius = (float)Math.Sqrt(centerX * centerX + centerY * centerY);

            // Draw radial vignette gradient effect using concentric semi-transparent ellipses
            for (int i = 20; i > 0; i--)
            {
                float progress = i / 20f;  // 1 to 0 (from edge to center)
                float radius = maxRadius * progress;
                
                // Quadratic falloff from edges to center
                float vignetteAlpha = _vignetteStrength * (1f - progress) * (1f - progress);
                vignetteAlpha = Math.Min(0.8f, vignetteAlpha);

                int alpha = (int)(255 * vignetteAlpha);
                
                using (Brush brush = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0)))
                {
                    g.FillEllipse(brush, centerX - radius, centerY - radius, radius * 2, radius * 2);
                }
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}