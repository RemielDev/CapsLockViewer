using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Windows.ApplicationModel;
using Windows.Storage;

namespace CapsLockViewer;

internal static class Program
{
    private const string AppName = "CapsLockViewer";

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length >= 1 && args[0] == "--export-preview")
            return IconFactory.ExportPreview(args.Length >= 2 ? args[1] : "preview");
        if (args.Length >= 1 && args[0] == "--export-store-assets")
            return IconFactory.ExportStoreAssets(args.Length >= 2 ? args[1] : "Assets");

        using var mutex = new Mutex(true, $"Local\\{AppName}.SingleInstance.v1", out bool first);
        if (!first) return 0;

        ApplicationConfiguration.Initialize();
        Application.Run(new TrayContext());
        GC.KeepAlive(mutex);
        return 0;
    }

    public static readonly bool IsPackaged = DetectPackaged();
    private static bool DetectPackaged()
    {
        try { _ = Package.Current.Id; return true; }
        catch { return false; }
    }
}

internal sealed class TrayContext : ApplicationContext
{
    private const int VK_CAPITAL = 0x14;
    private const string RUN_KEY = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string APP_KEY = @"Software\CapsLockViewer";
    private const string VALUE_NAME = "CapsLockViewer";
    private const string STARTUP_TASK_ID = "CapsLockViewer";

    [DllImport("user32.dll")] private static extern short GetKeyState(int vk);
    [DllImport("psapi.dll")] private static extern int EmptyWorkingSet(IntPtr h);
    [DllImport("kernel32.dll")] private static extern IntPtr GetCurrentProcess();
    private static void TrimWorkingSet() => EmptyWorkingSet(GetCurrentProcess());

    private readonly NotifyIcon _notify;
    private readonly System.Windows.Forms.Timer _timer;
    private readonly Icon _iconOn;
    private readonly Icon _iconOff;
    private readonly ToolStripMenuItem _startupItem;
    private readonly ToolStripMenuItem _hideWhenOffItem;
    private bool _lastOn;
    private bool _hideWhenOff;
    private int _ticks;

    public TrayContext()
    {
        _iconOn = IconFactory.Build(true);
        _iconOff = IconFactory.Build(false);

        EnsureFirstRunDefaults();
        _hideWhenOff = ReadHideWhenOff();

        var menu = new ContextMenuStrip();
        _startupItem = new ToolStripMenuItem("Run at startup", null, OnToggleStartup) { Checked = IsStartupEnabled() };
        _hideWhenOffItem = new ToolStripMenuItem("Hide icon when Caps Lock is off", null, OnToggleHide) { Checked = _hideWhenOff };
        menu.Opening += (_, _) => _startupItem.Checked = IsStartupEnabled();
        menu.Items.Add(_startupItem);
        menu.Items.Add(_hideWhenOffItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(new ToolStripMenuItem("Exit", null, (_, _) => ExitThread()));

        _notify = new NotifyIcon
        {
            Icon = _iconOff,
            Text = "Caps Lock: off",
            Visible = true,
            ContextMenuStrip = menu,
        };

        _timer = new System.Windows.Forms.Timer { Interval = 150 };
        _timer.Tick += OnTick;
        _timer.Start();
        Refresh(true);

        var trim = new System.Windows.Forms.Timer { Interval = 2000 };
        trim.Tick += (s, _) => { TrimWorkingSet(); ((System.Windows.Forms.Timer)s!).Dispose(); };
        trim.Start();
    }

    private void OnTick(object? sender, EventArgs e)
    {
        Refresh(false);
        if (++_ticks >= 400) { _ticks = 0; TrimWorkingSet(); }
    }

    private void Refresh(bool force)
    {
        bool on = (GetKeyState(VK_CAPITAL) & 1) != 0;
        if (!force && on == _lastOn) return;
        _lastOn = on;
        _notify.Icon = on ? _iconOn : _iconOff;
        _notify.Text = on ? "Caps Lock: ON" : "Caps Lock: off";
        _notify.Visible = on || !_hideWhenOff;
    }

    private async void OnToggleStartup(object? sender, EventArgs e)
    {
        await SetStartupEnabledAsync(!_startupItem.Checked);
        _startupItem.Checked = IsStartupEnabled();
    }

    private void OnToggleHide(object? sender, EventArgs e)
    {
        _hideWhenOff = !_hideWhenOff;
        _hideWhenOffItem.Checked = _hideWhenOff;
        WriteHideWhenOff(_hideWhenOff);
        Refresh(true);
    }

    private static string ExePath() => Environment.ProcessPath ?? Application.ExecutablePath;

    private static bool IsStartupEnabled()
    {
        if (Program.IsPackaged)
        {
            try
            {
                var t = StartupTask.GetAsync(STARTUP_TASK_ID).AsTask().GetAwaiter().GetResult();
                return t.State is StartupTaskState.Enabled or StartupTaskState.EnabledByPolicy;
            }
            catch { return false; }
        }
        using var k = Registry.CurrentUser.OpenSubKey(RUN_KEY);
        return k?.GetValue(VALUE_NAME) is string s && !string.IsNullOrWhiteSpace(s);
    }

    private static async Task SetStartupEnabledAsync(bool enabled)
    {
        if (Program.IsPackaged)
        {
            try
            {
                var t = await StartupTask.GetAsync(STARTUP_TASK_ID);
                if (enabled) await t.RequestEnableAsync();
                else t.Disable();
            }
            catch { }
            return;
        }
        using var k = Registry.CurrentUser.CreateSubKey(RUN_KEY, true)!;
        if (enabled) k.SetValue(VALUE_NAME, $"\"{ExePath()}\"", RegistryValueKind.String);
        else if (k.GetValue(VALUE_NAME) != null) k.DeleteValue(VALUE_NAME, false);
    }

    private static void EnsureFirstRunDefaults()
    {
        if (Program.IsPackaged)
        {
            var v = ApplicationData.Current.LocalSettings.Values;
            if (v["FirstRunCompleted"] as string == "1") return;
            try { _ = StartupTask.GetAsync(STARTUP_TASK_ID).AsTask().GetAwaiter().GetResult().RequestEnableAsync().AsTask().GetAwaiter().GetResult(); }
            catch { }
            v["FirstRunCompleted"] = "1";
            return;
        }
        using var k = Registry.CurrentUser.CreateSubKey(APP_KEY, true)!;
        if (k.GetValue("FirstRunCompleted") as string == "1") return;
        SetStartupEnabledAsync(true).GetAwaiter().GetResult();
        k.SetValue("FirstRunCompleted", "1", RegistryValueKind.String);
    }

    private static bool ReadHideWhenOff()
    {
        if (Program.IsPackaged)
            return ApplicationData.Current.LocalSettings.Values["HideWhenOff"] as string == "1";
        using var k = Registry.CurrentUser.OpenSubKey(APP_KEY);
        return k?.GetValue("HideWhenOff") as string == "1";
    }

    private static void WriteHideWhenOff(bool value)
    {
        var v = value ? "1" : "0";
        if (Program.IsPackaged)
        {
            ApplicationData.Current.LocalSettings.Values["HideWhenOff"] = v;
            return;
        }
        using var k = Registry.CurrentUser.CreateSubKey(APP_KEY, true)!;
        k.SetValue("HideWhenOff", v, RegistryValueKind.String);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _timer.Stop();
            _timer.Dispose();
            _notify.Visible = false;
            _notify.Dispose();
            _iconOn.Dispose();
            _iconOff.Dispose();
        }
        base.Dispose(disposing);
    }
}

internal static class IconFactory
{
    private static readonly int[] TraySizes = { 16, 20, 24, 32, 40, 48 };

    public static Icon Build(bool filled)
    {
        var pngs = TraySizes.Select(s => RenderSquarePng(s, filled)).ToArray();
        return BuildIco(TraySizes, pngs);
    }

    private static Icon BuildIco(int[] sizes, byte[][] pngs)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        bw.Write((short)0);
        bw.Write((short)1);
        bw.Write((short)pngs.Length);

        int offset = 6 + 16 * pngs.Length;
        for (int i = 0; i < pngs.Length; i++)
        {
            int s = sizes[i];
            bw.Write((byte)(s >= 256 ? 0 : s));
            bw.Write((byte)(s >= 256 ? 0 : s));
            bw.Write((byte)0);
            bw.Write((byte)0);
            bw.Write((short)1);
            bw.Write((short)32);
            bw.Write(pngs[i].Length);
            bw.Write(offset);
            offset += pngs[i].Length;
        }
        foreach (var p in pngs) bw.Write(p);
        ms.Position = 0;
        return new Icon(ms);
    }

    public static int ExportPreview(string dir)
    {
        try
        {
            Directory.CreateDirectory(dir);
            foreach (var s in new[] { 16, 24, 32, 64, 128 })
            {
                File.WriteAllBytes(Path.Combine(dir, $"on-{s}.png"), RenderSquarePng(s, true));
                File.WriteAllBytes(Path.Combine(dir, $"off-{s}.png"), RenderSquarePng(s, false));
            }
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    public static int ExportStoreAssets(string dir)
    {
        try
        {
            Directory.CreateDirectory(dir);
            (string name, int w, int h)[] tiles =
            {
                ("StoreLogo.png",          50,  50),
                ("Square44x44Logo.png",    44,  44),
                ("Square71x71Logo.png",    71,  71),
                ("Square150x150Logo.png", 150, 150),
                ("Square310x310Logo.png", 310, 310),
                ("Wide310x150Logo.png",   310, 150),
                ("SplashScreen.png",      620, 300),
            };
            foreach (var (name, w, h) in tiles)
                File.WriteAllBytes(Path.Combine(dir, name), RenderTilePng(w, h));
            return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }

    private static byte[] RenderSquarePng(int size, bool filled)
    {
        using var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            ConfigureGraphics(g);
            g.Clear(Color.Transparent);
            DrawSquare(g, size, filled);
        }
        return ToPng(bmp);
    }

    private static byte[] RenderTilePng(int width, int height)
    {
        using var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bmp))
        {
            ConfigureGraphics(g);
            g.Clear(Color.Transparent);
            int side = (int)(Math.Min(width, height) * 0.78f);
            int x = (width - side) / 2;
            int y = (height - side) / 2;
            using var clip = new Bitmap(side, side, PixelFormat.Format32bppArgb);
            using (var cg = Graphics.FromImage(clip))
            {
                ConfigureGraphics(cg);
                DrawSquare(cg, side, true);
            }
            g.DrawImageUnscaled(clip, x, y);
        }
        return ToPng(bmp);
    }

    private static void ConfigureGraphics(Graphics g)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
    }

    private static void DrawSquare(Graphics g, int size, bool filled)
    {
        float pad = MathF.Max(1f, size * 0.06f);
        float radius = size * 0.22f;
        var rect = new RectangleF(pad, pad, size - 2 * pad, size - 2 * pad);

        using var path = RoundedRect(rect, radius);
        var onAccent = Color.FromArgb(255, 0, 168, 255);
        var offStroke = Color.FromArgb(235, 255, 255, 255);

        if (filled)
        {
            using var bg = new SolidBrush(onAccent);
            g.FillPath(bg, path);
        }
        else
        {
            float stroke = MathF.Max(1.2f, size * 0.07f);
            using var pen = new Pen(offStroke, stroke) { LineJoin = LineJoin.Round };
            using var inset = RoundedRect(new RectangleF(rect.X + stroke / 2f, rect.Y + stroke / 2f, rect.Width - stroke, rect.Height - stroke), radius);
            g.DrawPath(pen, inset);
        }

        using var family = new FontFamily(GenericFontFamilies.SansSerif);
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var letter = new GraphicsPath();
        letter.AddString("A", family, (int)FontStyle.Bold, size * 0.72f,
            new RectangleF(0, -size * 0.04f, size, size), sf);
        using var fg = new SolidBrush(filled ? Color.White : offStroke);
        g.FillPath(fg, letter);
    }

    private static GraphicsPath RoundedRect(RectangleF r, float radius)
    {
        float d = radius * 2f;
        var p = new GraphicsPath();
        p.AddArc(r.X, r.Y, d, d, 180, 90);
        p.AddArc(r.Right - d, r.Y, d, d, 270, 90);
        p.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
        p.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
        p.CloseFigure();
        return p;
    }

    private static byte[] ToPng(Bitmap bmp)
    {
        using var ms = new MemoryStream();
        bmp.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }
}
