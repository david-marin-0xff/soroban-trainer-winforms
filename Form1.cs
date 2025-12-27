/* using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SorobanWinForms
{
    public partial class Form1 : Form
    {
        private Abacus abacus = null!;
        private Panel canvas = null!;
        private Label valueLabel = null!;
        private ComboBox themeBox = null!;
        private TrackBar fontSizeBar = null!;
        private CheckBox middleStart = null!;
        private Button resetBtn = null!;

        public Form1()
        {
            InitializeComponent();
            Text = "Soroban Trainer";
            Width = 1200;
            Height = 520;

            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 70,
                Padding = new Padding(10),
                FlowDirection = FlowDirection.LeftToRight
            };

            valueLabel = new Label
            {
                Text = "0",
                Width = 360,
                Font = new Font("Consolas", 24, FontStyle.Bold)
            };

            fontSizeBar = new TrackBar
            {
                Minimum = 12,
                Maximum = 48,
                Value = 24,
                Width = 150
            };
            fontSizeBar.ValueChanged += (_, __) =>
                valueLabel.Font = new Font("Consolas", fontSizeBar.Value, FontStyle.Bold);

            themeBox = new ComboBox
            {
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            themeBox.Items.AddRange(Enum.GetNames(typeof(Theme)));
            themeBox.SelectedIndex = 0;
            themeBox.SelectedIndexChanged += (_, __) => canvas.Invalidate();

            middleStart = new CheckBox { Text = "Middle = Units (decimal)", AutoSize = true };
            middleStart.CheckedChanged += (_, __) =>
            {
                abacus.MiddleAsUnits = middleStart.Checked;
                canvas.Invalidate();
            };

            resetBtn = new Button { Text = "Reset", Width = 80 };
            resetBtn.Click += (_, __) =>
            {
                abacus.Reset();
                canvas.Invalidate();
            };

            top.Controls.Add(valueLabel);
            top.Controls.Add(new Label { Text = "Font", AutoSize = true, Padding = new Padding(5, 10, 0, 0) });
            top.Controls.Add(fontSizeBar);
            top.Controls.Add(themeBox);
            top.Controls.Add(middleStart);
            top.Controls.Add(resetBtn);

            canvas = new Panel { Dock = DockStyle.Fill };
            canvas.Paint += Canvas_Paint;
            canvas.MouseDown += Canvas_MouseDown;

            Controls.Add(canvas);
            Controls.Add(top);

            abacus = new Abacus(13);
            DoubleBuffered = true;
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            ThemeColors.Apply((Theme)themeBox.SelectedIndex);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            abacus.Draw(e.Graphics);
            valueLabel.Text = abacus.GetFormattedValue();
        }

        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (abacus.Click(e.Location))
                canvas.Invalidate();
        }
    }

    // ================= ABACUS =================

    class Abacus
    {
        public List<Rod> Rods = new();
        public bool MiddleAsUnits;

        public Abacus(int count)
        {
            for (int i = 0; i < count; i++)
                Rods.Add(new Rod(i));
        }

        public void Reset()
        {
            foreach (var r in Rods)
                r.Reset();
        }

        public bool Click(Point p)
        {
            foreach (var r in Rods)
                if (r.Click(p))
                    return true;
            return false;
        }

        public string GetFormattedValue()
        {
            double total = 0;
            int mid = Rods.Count / 2;

            for (int i = 0; i < Rods.Count; i++)
            {
                int power = MiddleAsUnits ? mid - i : Rods.Count - 1 - i;
                total += Rods[i].Value * Math.Pow(10, power);
            }

            return MiddleAsUnits ? total.ToString("0.###") : total.ToString();
        }

        public void Draw(Graphics g)
        {
            g.Clear(ThemeColors.Background);

            // divider bar
            using var bar = new Pen(ThemeColors.Bar, 5);
            g.DrawLine(bar, 0, 180, 2000, 180);

            foreach (var r in Rods)
                r.Draw(g);
        }
    }

    // ================= ROD =================

    class Rod
    {
        public int Index;
        public bool Heaven;
        public int EarthCount;

        public Rod(int index)
        {
            Index = index;
        }

        public float X => 50 + Index * 75;

        public int Value => (Heaven ? 5 : 0) + EarthCount;

        public void Reset()
        {
            Heaven = false;
            EarthCount = 0;
        }

        public bool Click(Point p)
        {
            // Heaven bead
            var heavenRect = new RectangleF(X - 22, Heaven ? 145 : 115, 44, 24);
            if (heavenRect.Contains(p))
            {
                Heaven = !Heaven;
                return true;
            }

            // Earth beads (snap BOTH ways)
            for (int i = 1; i <= 4; i++)
            {
                float y = 200 + i * 32 - (i <= EarthCount ? 32 : 0);
                var rect = new RectangleF(X - 22, y, 44, 24);
                if (rect.Contains(p))
                {
                    EarthCount = (EarthCount == i) ? i - 1 : i;
                    return true;
                }
            }
            return false;
        }

        public void Draw(Graphics g)
        {
            using var rodPen = new Pen(ThemeColors.Rod, 4);
            g.DrawLine(rodPen, X, 90, X, 430);

            // Heaven bead
            DrawBead(g, Heaven, X, Heaven ? 145 : 115);

            // Earth beads
            for (int i = 1; i <= 4; i++)
            {
                bool active = i <= EarthCount;
                float y = 200 + i * 32 - (active ? 32 : 0);
                DrawBead(g, active, X, y);
            }
        }

        private void DrawBead(Graphics g, bool active, float x, float y)
        {
            using var b = new SolidBrush(active ? ThemeColors.BeadActive : ThemeColors.Bead);
            g.FillEllipse(b, x - 22, y, 44, 24);
            g.DrawEllipse(Pens.Black, x - 22, y, 44, 24);
        }
    }

    // ================= THEMES =================

    enum Theme
    {
        ClassicWood,
        Dark,
        BlueSteel,
        Jade,
        Crimson,
        Solarized,
        Midnight,
        Ivory,
        HighContrast,
        Retro
    }

    static class ThemeColors
    {
        public static Color Background, Rod, Bead, BeadActive, Bar;

        public static void Apply(Theme t)
        {
            (Background, Rod, Bead, BeadActive, Bar) = t switch
            {
                Theme.Dark => (Color.Black, Color.Gray, Color.DimGray, Color.Orange, Color.DarkGray),
                Theme.BlueSteel => (Color.WhiteSmoke, Color.Navy, Color.LightSteelBlue, Color.Gold, Color.Navy),
                Theme.Jade => (Color.Honeydew, Color.DarkGreen, Color.PaleGreen, Color.ForestGreen, Color.DarkGreen),
                Theme.Crimson => (Color.MistyRose, Color.DarkRed, Color.LightCoral, Color.Firebrick, Color.DarkRed),
                Theme.Solarized => (Color.Beige, Color.SaddleBrown, Color.Khaki, Color.OrangeRed, Color.Brown),
                Theme.Midnight => (Color.FromArgb(15,15,30), Color.SlateBlue, Color.SteelBlue, Color.Cyan, Color.SlateBlue),
                Theme.Ivory => (Color.Ivory, Color.Sienna, Color.Bisque, Color.Peru, Color.Sienna),
                Theme.HighContrast => (Color.White, Color.Black, Color.White, Color.Red, Color.Black),
                Theme.Retro => (Color.LightYellow, Color.Maroon, Color.Tan, Color.DarkOrange, Color.Maroon),
                _ => (Color.FromArgb(245,235,220), Color.SaddleBrown, Color.Bisque, Color.Sienna, Color.SaddleBrown)
            };
        }
    }
}
*/
/* Full version below: */


using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SorobanWinForms
{
    public partial class Form1 : Form
    {
        private Abacus abacus = null!;
        private Panel canvas = null!;
        private Label valueLabel = null!;
        private ComboBox themeBox = null!;
        private TrackBar fontSizeBar = null!;
        private CheckBox middleStart = null!;
        private Button resetBtn = null!;

        public Form1()
        {
            InitializeComponent();
            Text = "Soroban Trainer";
            Width = 1200;
            Height = 520;

            // ===== TOP BAR =====
            var top = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false
            };

            valueLabel = new Label
            {
                Text = "0",
                AutoSize = true,
                Font = new Font("Consolas", 24, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Container so label can grow without clipping
            var valuePanel = new Panel
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(5, 10, 20, 10)
            };
            valuePanel.Controls.Add(valueLabel);

            fontSizeBar = new TrackBar
            {
                Minimum = 12,
                Maximum = 48,
                Value = 24,
                Width = 150
            };
            fontSizeBar.ValueChanged += (_, __) =>
            {
                valueLabel.Font = new Font("Consolas", fontSizeBar.Value, FontStyle.Bold);
                valueLabel.Parent?.PerformLayout();
            };

            themeBox = new ComboBox
            {
                Width = 160,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            themeBox.Items.AddRange(Enum.GetNames(typeof(Theme)));
            themeBox.SelectedIndex = 0;
            themeBox.SelectedIndexChanged += (_, __) => canvas.Invalidate();

            middleStart = new CheckBox
            {
                Text = "Middle = Units (decimal)",
                AutoSize = true
            };
            middleStart.CheckedChanged += (_, __) =>
            {
                abacus.MiddleAsUnits = middleStart.Checked;
                canvas.Invalidate();
            };

            resetBtn = new Button
            {
                Text = "Reset",
                Width = 80
            };
            resetBtn.Click += (_, __) =>
            {
                abacus.Reset();
                canvas.Invalidate();
            };

            top.Controls.Add(valuePanel);
            top.Controls.Add(new Label { Text = "Font", AutoSize = true, Padding = new Padding(5, 12, 0, 0) });
            top.Controls.Add(fontSizeBar);
            top.Controls.Add(themeBox);
            top.Controls.Add(middleStart);
            top.Controls.Add(resetBtn);

            // ===== CANVAS =====
            canvas = new Panel { Dock = DockStyle.Fill };
            canvas.Paint += Canvas_Paint;
            canvas.MouseDown += Canvas_MouseDown;

            Controls.Add(canvas);
            Controls.Add(top);

            abacus = new Abacus(13);
            DoubleBuffered = true;
        }

        private void Canvas_Paint(object? sender, PaintEventArgs e)
        {
            ThemeColors.Apply((Theme)themeBox.SelectedIndex);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            abacus.Draw(e.Graphics);
            valueLabel.Text = abacus.GetFormattedValue();
        }

        private void Canvas_MouseDown(object? sender, MouseEventArgs e)
        {
            if (abacus.Click(e.Location))
                canvas.Invalidate();
        }
    }

    // ================= ABACUS =================

    class Abacus
    {
        public List<Rod> Rods = new();
        public bool MiddleAsUnits;

        public Abacus(int count)
        {
            for (int i = 0; i < count; i++)
                Rods.Add(new Rod(i));
        }

        public void Reset()
        {
            foreach (var r in Rods)
                r.Reset();
        }

        public bool Click(Point p)
        {
            foreach (var r in Rods)
                if (r.Click(p))
                    return true;
            return false;
        }

        public string GetFormattedValue()
        {
            double total = 0;
            int mid = Rods.Count / 2;

            for (int i = 0; i < Rods.Count; i++)
            {
                int power = MiddleAsUnits ? mid - i : Rods.Count - 1 - i;
                total += Rods[i].Value * Math.Pow(10, power);
            }

            return MiddleAsUnits ? total.ToString("0.###") : total.ToString();
        }

        public void Draw(Graphics g)
        {
            g.Clear(ThemeColors.Background);

            using var bar = new Pen(ThemeColors.Bar, 5);
            g.DrawLine(bar, 0, 180, 2000, 180);

            foreach (var r in Rods)
                r.Draw(g);
        }
    }

    // ================= ROD =================

    class Rod
    {
        public int Index;
        public bool Heaven;
        public int EarthCount;

        public Rod(int index)
        {
            Index = index;
        }

        public float X => 50 + Index * 75;

        public int Value => (Heaven ? 5 : 0) + EarthCount;

        public void Reset()
        {
            Heaven = false;
            EarthCount = 0;
        }

        public bool Click(Point p)
        {
            var heavenRect = new RectangleF(X - 22, Heaven ? 145 : 115, 44, 24);
            if (heavenRect.Contains(p))
            {
                Heaven = !Heaven;
                return true;
            }

            for (int i = 1; i <= 4; i++)
            {
                float y = 200 + i * 32 - (i <= EarthCount ? 32 : 0);
                var rect = new RectangleF(X - 22, y, 44, 24);
                if (rect.Contains(p))
                {
                    EarthCount = (EarthCount == i) ? i - 1 : i;
                    return true;
                }
            }
            return false;
        }

        public void Draw(Graphics g)
        {
            using var rodPen = new Pen(ThemeColors.Rod, 4);
            g.DrawLine(rodPen, X, 90, X, 430);

            DrawBead(g, Heaven, X, Heaven ? 145 : 115);

            for (int i = 1; i <= 4; i++)
            {
                bool active = i <= EarthCount;
                float y = 200 + i * 32 - (active ? 32 : 0);
                DrawBead(g, active, X, y);
            }
        }

        private void DrawBead(Graphics g, bool active, float x, float y)
        {
            using var b = new SolidBrush(active ? ThemeColors.BeadActive : ThemeColors.Bead);
            g.FillEllipse(b, x - 22, y, 44, 24);
            g.DrawEllipse(Pens.Black, x - 22, y, 44, 24);
        }
    }

    // ================= THEMES =================

    enum Theme
    {
        ClassicWood,
        Dark,
        BlueSteel,
        Jade,
        Crimson,
        Solarized,
        Midnight,
        Ivory,
        HighContrast,
        Retro
    }

    static class ThemeColors
    {
        public static Color Background, Rod, Bead, BeadActive, Bar;

        public static void Apply(Theme t)
        {
            (Background, Rod, Bead, BeadActive, Bar) = t switch
            {
                Theme.Dark => (Color.Black, Color.Gray, Color.DimGray, Color.Orange, Color.DarkGray),
                Theme.BlueSteel => (Color.WhiteSmoke, Color.Navy, Color.LightSteelBlue, Color.Gold, Color.Navy),
                Theme.Jade => (Color.Honeydew, Color.DarkGreen, Color.PaleGreen, Color.ForestGreen, Color.DarkGreen),
                Theme.Crimson => (Color.MistyRose, Color.DarkRed, Color.LightCoral, Color.Firebrick, Color.DarkRed),
                Theme.Solarized => (Color.Beige, Color.SaddleBrown, Color.Khaki, Color.OrangeRed, Color.Brown),
                Theme.Midnight => (Color.FromArgb(15, 15, 30), Color.SlateBlue, Color.SteelBlue, Color.Cyan, Color.SlateBlue),
                Theme.Ivory => (Color.Ivory, Color.Sienna, Color.Bisque, Color.Peru, Color.Sienna),
                Theme.HighContrast => (Color.White, Color.Black, Color.White, Color.Red, Color.Black),
                Theme.Retro => (Color.LightYellow, Color.Maroon, Color.Tan, Color.DarkOrange, Color.Maroon),
                _ => (Color.FromArgb(245, 235, 220), Color.SaddleBrown, Color.Bisque, Color.Sienna, Color.SaddleBrown)
            };
        }
    }
}
