"""Generates the Eclipse 9-16 (Karma Climb) medal art.

The vanilla Eclipse medals have their numerals painted into the sprite and stop at 8, so levels 9-16
need their own art. Each medal here is built from the Eclipse 8 medal, the most decorated of the set:

1. the "8" is erased by clearing the disc inside the medal's inner ring,
2. the new number is drawn in Bombardier, RoR2's UI font, with the same three layers the vanilla
   numerals use (fill, semi-transparent black outline, white outer stroke),
3. for the earned state, the ring's blue is moved to the Karma icon blue before the numeral goes on
   (so the numeral keeps the pale fill Karma icons use), then the medal gets the Karma glow.

Two states are written per level, matching how Karma icons work: Active (earned) and Inactive (not yet
earned, built from the vanilla Unearned medal).

The source art and font are read from the local game install and are not committed; only the
generated PNGs are.

Usage (needs UnityPy and Pillow):
    python tools/generate_medals.py "<Risk of Rain 2 install dir>" KarmaClimb/Assets/Medals
"""

import colorsys
import io
import math
import os
import sys

import UnityPy
from PIL import Image, ImageDraw, ImageFilter, ImageFont

# Geometry of the vanilla 128px medal, measured from texDifficultyEclipse8Icon.
CENTER = (72, 69)        # centre of the numeral area
ERASE_RADIUS = 34        # clears the numeral; the inner ring starts at ~37
FIT_RADIUS = 32          # every pixel of the new numeral sits inside this, clear of the ring
SINGLE_DIGIT_HEIGHT = 57 # full height of the vanilla "8", strokes included

# Numeral layers, sampled from the vanilla medals.
OUTLINE_WIDTH = 4
STROKE_WIDTH = 3
# The earned numeral is drawn after the recolour, in the pale blue Karma icons use for theirs, so it
# keeps its contrast against the saturated ring.
EARNED_NUMERAL = dict(fill=(196, 214, 255, 255), outline=(0, 0, 0, 160), stroke=(250, 250, 250, 228))
UNEARNED_NUMERAL = dict(fill=(10, 12, 10, 255), outline=(0, 0, 0, 160), stroke=(55, 55, 55, 225))

# Karma icon palette, sampled from texKarmaIconLevelNActive.
KARMA_HUE = 223 / 360
SATURATION_GAIN = 2.0    # vanilla medal blue is ~0.33 saturated, the Karma blue ~0.65
GLOW_COLOR = (80, 120, 230)
GLOW_RADIUS = 3
GLOW_OPACITY = 0.55

SUPERSAMPLE = 4
LEVELS = range(9, 17)


def load_sources(game_dir):
    """Pulls the Eclipse 8 medal textures and the Bombardier font out of the game's bundles."""
    bundle_dir = os.path.join(game_dir, "Risk of Rain 2_Data", "StreamingAssets", "aa", "StandaloneWindows64")
    wanted = {"texDifficultyEclipse8Icon", "texDifficultyEclipse8IconUnearned"}
    textures, font_bytes = {}, None

    for name in os.listdir(bundle_dir):
        if not name.startswith(("ror2-base-eclipserun_", "ror2-base-common-fonts-bombardier_")):
            continue
        for obj in UnityPy.load(os.path.join(bundle_dir, name)).objects:
            if obj.type.name == "Texture2D":
                data = obj.read()
                if data.m_Name in wanted:
                    textures[data.m_Name] = data.image.convert("RGBA")
            elif obj.type.name == "Font":
                data = obj.read()
                if data.m_FontData:
                    font_bytes = bytes(data.m_FontData)

    missing = wanted - textures.keys()
    if missing or not font_bytes:
        sys.exit(f"Missing from the game install: {sorted(missing) or ''} {'' if font_bytes else 'Bombardier font'}")

    return textures["texDifficultyEclipse8Icon"], textures["texDifficultyEclipse8IconUnearned"], font_bytes


def erase_numeral(medal):
    mask = Image.new("L", medal.size, 0)
    cx, cy = CENTER
    ImageDraw.Draw(mask).ellipse([cx - ERASE_RADIUS, cy - ERASE_RADIUS, cx + ERASE_RADIUS, cy + ERASE_RADIUS], fill=255)
    cleared = medal.copy()
    cleared.paste((0, 0, 0, 0), (0, 0) + medal.size, mask)
    return cleared


def render_numeral(text, font_bytes, size, colors, condense, line_scale):
    """Renders the numeral with its three layers at supersampled resolution, centred on CENTER.

    line_scale shrinks the outline and stroke along with the font: at a fixed width they swallowed
    the thinner strokes of the smaller two-digit numerals.
    """
    s = SUPERSAMPLE
    font = ImageFont.truetype(io.BytesIO(font_bytes), size * s)
    outline = max(2, round(OUTLINE_WIDTH * line_scale))
    stroke = max(2, round(STROKE_WIDTH * line_scale))

    # Draw onto a generous scratch canvas, then crop to the ink and condense horizontally if asked.
    scratch = Image.new("RGBA", (128 * s * 2, 128 * s * 2), (0, 0, 0, 0))
    draw = ImageDraw.Draw(scratch)
    origin = (scratch.width // 4, scratch.height // 4)
    draw.text(origin, text, font=font, fill=colors["stroke"],
              stroke_width=(outline + stroke) * s, stroke_fill=colors["stroke"])
    draw.text(origin, text, font=font, fill=colors["fill"],
              stroke_width=outline * s, stroke_fill=colors["outline"])

    ink = scratch.crop(scratch.getbbox())
    if condense != 1.0:
        ink = ink.resize((max(1, round(ink.width * condense)), ink.height), Image.LANCZOS)

    layer = Image.new("RGBA", (128 * s, 128 * s), (0, 0, 0, 0))
    cx, cy = CENTER
    layer.alpha_composite(ink, (round(cx * s - ink.width / 2), round(cy * s - ink.height / 2)))
    return layer.resize((128, 128), Image.LANCZOS)


def fits(numeral):
    cx, cy = CENTER
    alpha = numeral.getchannel("A")
    for y in range(numeral.height):
        for x in range(numeral.width):
            if alpha.getpixel((x, y)) > 24 and math.hypot(x - cx, y - cy) > FIT_RADIUS:
                return False
    return True


def numeral_height(numeral):
    box = numeral.getchannel("A").point(lambda a: 255 if a > 24 else 0).getbbox()
    return box[3] - box[1] if box else 0


def reference_size(font_bytes):
    """Font size at which a single digit matches the vanilla numeral's height."""
    size = 20
    while numeral_height(render_numeral("8", font_bytes, size + 1, EARNED_NUMERAL, 1.0, 1.0)) <= SINGLE_DIGIT_HEIGHT:
        size += 1
    return size


def fit_numeral(text, font_bytes, colors, ref_size):
    """Largest numeral that fits inside the ring, capped at the vanilla single-digit height."""
    condense = 1.0 if len(text) == 1 else 0.9
    best = None
    for size in range(20, ref_size + 1):
        numeral = render_numeral(text, font_bytes, size, colors, condense, size / ref_size)
        if not fits(numeral) or numeral_height(numeral) > SINGLE_DIGIT_HEIGHT:
            break
        best = numeral
    if best is None:
        sys.exit(f"Could not fit {text!r} inside the medal ring")
    return best


def to_karma_palette(image):
    """Moves every coloured pixel to the Karma hue and saturation; whites, greys and blacks are kept."""
    out = image.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            h, s, v = colorsys.rgb_to_hsv(r / 255, g / 255, b / 255)
            if s < 0.08:
                continue
            r2, g2, b2 = colorsys.hsv_to_rgb(KARMA_HUE, min(1.0, s * SATURATION_GAIN), v)
            px[x, y] = (round(r2 * 255), round(g2 * 255), round(b2 * 255), a)
    return out


def add_glow(image):
    alpha = image.getchannel("A").filter(ImageFilter.GaussianBlur(GLOW_RADIUS))
    alpha = alpha.point(lambda a: round(a * GLOW_OPACITY))
    glow = Image.new("RGBA", image.size, GLOW_COLOR + (0,))
    glow.putalpha(alpha)
    glow.alpha_composite(image)
    return glow


def main():
    if len(sys.argv) != 3:
        sys.exit(__doc__)

    game_dir, out_dir = sys.argv[1], sys.argv[2]
    os.makedirs(out_dir, exist_ok=True)

    earned_base, unearned_base, font_bytes = load_sources(game_dir)
    earned_base = to_karma_palette(erase_numeral(earned_base))
    unearned_base = erase_numeral(unearned_base)
    ref_size = reference_size(font_bytes)

    for level in LEVELS:
        text = str(level)

        # Numeral after the recolour, so it keeps its own pale fill; glow last, over the whole medal.
        active = earned_base.copy()
        active.alpha_composite(fit_numeral(text, font_bytes, EARNED_NUMERAL, ref_size))
        active = add_glow(active)
        active.save(os.path.join(out_dir, f"texKarmaClimbMedal{level}Active.png"))

        inactive = unearned_base.copy()
        inactive.alpha_composite(fit_numeral(text, font_bytes, UNEARNED_NUMERAL, ref_size))
        inactive.save(os.path.join(out_dir, f"texKarmaClimbMedal{level}Inactive.png"))

        print(f"Eclipse {level}: active + inactive")


if __name__ == "__main__":
    main()
