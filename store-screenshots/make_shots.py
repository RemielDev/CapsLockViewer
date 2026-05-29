"""Generate Microsoft Store screenshots for Caps Lock Viewer from the real icon assets."""
import os
from PIL import Image, ImageDraw, ImageFont, ImageFilter

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
PREVIEW = os.path.join(ROOT, "preview")
OUT = os.path.join(ROOT, "store-screenshots")
os.makedirs(OUT, exist_ok=True)

W, H = 1920, 1080
FONTS = "C:/Windows/Fonts/"
def font(name, size): return ImageFont.truetype(FONTS + name, size)

f_title   = font("segoeuib.ttf", 76)
f_sub     = font("segoeui.ttf", 30)
f_cap     = font("seguisb.ttf", 30)
f_menu    = font("segoeui.ttf", 23)
f_clock   = font("segoeui.ttf", 17)
f_clock2  = font("segoeui.ttf", 15)
f_label   = font("seguisb.ttf", 34)
f_small   = font("segoeui.ttf", 24)

ON  = Image.open(os.path.join(PREVIEW, "on-128.png")).convert("RGBA")
OFF = Image.open(os.path.join(PREVIEW, "off-128.png")).convert("RGBA")

def wallpaper():
    """Windows 11 style blue/teal bloom gradient."""
    bg = Image.new("RGB", (W, H), (12, 18, 38))
    top, bot = (20, 40, 92), (8, 12, 28)
    px = bg.load()
    for y in range(H):
        t = y / H
        px2 = tuple(int(top[i] + (bot[i] - top[i]) * t) for i in range(3))
        for x in range(W):
            px[x, y] = px2
    # soft radial bloom
    bloom = Image.new("L", (W, H), 0)
    bd = ImageDraw.Draw(bloom)
    bd.ellipse([W*0.30, -H*0.45, W*1.05, H*0.75], fill=120)
    bloom = bloom.filter(ImageFilter.GaussianBlur(220))
    glow = Image.new("RGB", (W, H), (40, 120, 210))
    bg = Image.composite(glow, bg, bloom)
    return bg.convert("RGBA")

def rrect(d, box, r, fill=None, outline=None, width=1):
    d.rounded_rectangle(box, radius=r, fill=fill, outline=outline, width=width)

def paste(base, img, xy):
    base.alpha_composite(img, xy)

def start_logo(size):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    g = int(size * 0.10); c = (96, 205, 255, 255)
    cell = (size - g) // 2
    for ox, oy in [(0, 0), (cell + g, 0), (0, cell + g), (cell + g, cell + g)]:
        d.rounded_rectangle([ox, oy, ox + cell - g, oy + cell - g], radius=int(cell*0.16), fill=c)
    return img

def taskbar(base, highlight=True):
    d = ImageDraw.Draw(base)
    bar = Image.new("RGBA", (W, 60), (24, 24, 28, 245))
    paste(base, bar, (0, H - 60))
    d.line([(0, H - 60), (W, H - 60)], fill=(255, 255, 255, 22), width=1)
    cy = H - 30
    # centered cluster: start + a few neutral app glyphs
    sl = start_logo(26); paste(base, sl, (W//2 - 90, cy - 13))
    for i, col in enumerate([(120,170,235),(235,180,120),(150,220,170),(210,150,210)]):
        x = W//2 - 40 + i*34
        d.rounded_rectangle([x, cy-12, x+24, cy+12], radius=6, fill=col + (235,))
    # right tray
    tx = W - 300
    # chevron
    d.line([(tx, cy+3),(tx+7, cy-4)], fill=(220,220,225,235), width=2)
    d.line([(tx+7, cy-4),(tx+14, cy+3)], fill=(220,220,225,235), width=2)
    # wifi (arc-ish)
    wx = tx + 38
    for rr, al in [(12,120),(8,180),(4,255)]:
        d.arc([wx-rr, cy-rr+4, wx+rr, cy+rr+4], 215, 325, fill=(225,225,230,al), width=2)
    d.ellipse([wx-2, cy+5, wx+2, cy+9], fill=(225,225,230,255))
    # speaker
    sx = tx + 74
    d.polygon([(sx-6,cy-3),(sx-1,cy-3),(sx+4,cy-8),(sx+4,cy+8),(sx-1,cy+3),(sx-6,cy+3)], fill=(225,225,230,235))
    d.arc([sx+4, cy-7, sx+14, cy+7], 300, 60, fill=(225,225,230,235), width=2)
    # OUR cyan A icon
    ax = tx + 112
    icon = ON.resize((26, 26), Image.LANCZOS)
    if highlight:
        d.rounded_rectangle([ax-7, cy-16, ax+33, cy+16], radius=8, fill=(255,255,255,28))
    paste(base, icon, (ax, cy - 13))
    # clock
    cd = ImageDraw.Draw(base)
    cd.text((W - 58, cy - 16), "10:48 AM", font=f_clock, fill=(235,235,238,255), anchor="mm")
    cd.text((W - 58, cy + 4), "5/28/2026", font=f_clock2, fill=(205,205,210,255), anchor="mm")
    return ax, cy

# ---------- Screenshot 1: hero ----------
def hero():
    base = wallpaper()
    d = ImageDraw.Draw(base)
    # centered icon
    big = ON.resize((200, 200), Image.LANCZOS)
    # soft glow behind icon
    glow = Image.new("RGBA", (W, H), (0,0,0,0))
    gd = ImageDraw.Draw(glow)
    gd.ellipse([W//2-170, 250, W//2+170, 590], fill=(0,180,255,90))
    glow = glow.filter(ImageFilter.GaussianBlur(70))
    base.alpha_composite(glow)
    paste(base, big, (W//2 - 100, 248))
    d.text((W//2, 520), "Caps Lock Viewer", font=f_title, fill=(245,247,250,255), anchor="mm")
    d.text((W//2, 588), "Caps Lock status in your system tray. About 10 MB of RAM.",
           font=f_sub, fill=(200,208,220,255), anchor="mm")
    ax, cy = taskbar(base, highlight=True)
    # caption pointing at tray
    d.text((W - 194, cy - 70), "lives here", font=f_clock, fill=(180,225,255,255), anchor="mm")
    d.line([(W-194, cy-58),(ax+13, cy-20)], fill=(120,200,255,200), width=2)
    base.convert("RGB").save(os.path.join(OUT, "01-hero.png"))

# ---------- Screenshot 2: right-click menu ----------
def menu_shot():
    base = wallpaper()
    d = ImageDraw.Draw(base)
    d.text((110, 90), "One right-click menu. That's the whole UI.",
           font=f_label, fill=(240,243,248,255))
    ax, cy = taskbar(base, highlight=True)
    # context menu above the tray icon
    mw, mh = 300, 160
    mx, my = W - 420, H - 60 - 40 - mh
    shadow = Image.new("RGBA", (W, H), (0,0,0,0))
    sd = ImageDraw.Draw(shadow)
    sd.rounded_rectangle([mx, my+6, mx+mw, my+mh+6], radius=12, fill=(0,0,0,120))
    shadow = shadow.filter(ImageFilter.GaussianBlur(16))
    base.alpha_composite(shadow)
    rrect(d, [mx, my, mx+mw, my+mh], 12, fill=(43,43,47,255), outline=(255,255,255,30), width=1)
    items = [("Run at startup", True), ("Hide icon when off", False)]
    yy = my + 16
    for text, checked in items:
        if checked:
            d.line([(mx+18, yy+13),(mx+23, yy+18)], fill=(120,205,255,255), width=2)
            d.line([(mx+23, yy+18),(mx+33, yy+6)], fill=(120,205,255,255), width=2)
        d.text((mx+46, yy+11), text, font=f_menu, fill=(235,235,240,255))
        yy += 40
    d.line([(mx+14, yy+4),(mx+mw-14, yy+4)], fill=(255,255,255,28), width=1)
    yy += 14
    d.text((mx+46, yy+11), "Exit", font=f_menu, fill=(235,235,240,255))
    base.convert("RGB").save(os.path.join(OUT, "02-menu.png"))

# ---------- Screenshot 3: ON vs OFF ----------
def states_shot():
    base = wallpaper()
    d = ImageDraw.Draw(base)
    d.text((W//2, 110), "Glance down. Know instantly.", font=f_label,
           fill=(240,243,248,255), anchor="mm")
    cards = [(ON, "ON", "solid cyan", (96,205,255)),
             (OFF, "OFF", "white outline", (200,205,215))]
    cw, ch = 420, 460
    gap = 80
    total = cw*2 + gap
    x0 = (W - total)//2
    y0 = 250
    for i, (icon, label, desc, col) in enumerate(cards):
        cx = x0 + i*(cw+gap)
        rrect(d, [cx, y0, cx+cw, y0+ch], 24, fill=(22,32,58,215), outline=(255,255,255,40), width=1)
        big = icon.resize((200, 200), Image.LANCZOS)
        paste(base, big, (cx + cw//2 - 100, y0 + 50))
        d.text((cx+cw//2, y0+320), label, font=f_label, fill=col+(255,), anchor="mm")
        d.text((cx+cw//2, y0+372), desc, font=f_small, fill=(195,200,210,255), anchor="mm")
    taskbar(base, highlight=True)
    base.convert("RGB").save(os.path.join(OUT, "03-states.png"))

hero()
menu_shot()
states_shot()
print("done:", os.listdir(OUT))
