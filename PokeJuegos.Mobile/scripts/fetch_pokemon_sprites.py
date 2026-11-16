#!/usr/bin/env python3
"""Downloads the 'official-artwork' sprite (front_default) for each pokemon
in Resources/Raw/pokemon_details.json, converts it to WebP and saves it to
Resources/Images/Sprites/ as pokemon_<id>.webp. Rewrites pokemon_details.json so
front_default holds that MAUI image name instead of the remote GitHub URL.

Sprites are kept at their original size (475x475) and registered in the
.csproj as MauiImage with Resize="false", so Resizetizer copies them as-is
instead of generating one copy per screen density. The "pokemon_" prefix is
required: Android resource names can't start with a digit.

Requires Pillow in addition to the stdlib: pip install pillow
Run once, by hand, after fetch_pokemon_details.py. Not part of the app's
normal build flow (dotnet build).

Usage:
    python3 scripts/fetch_pokemon_sprites.py
"""
import io
import json
import time
import urllib.error
import urllib.request
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
DETAILS_FILE = ROOT / "Resources" / "Raw" / "pokemon_details.json"
SPRITES_DIR = ROOT / "Resources" / "Images" / "Sprites"
REQUEST_DELAY_SECONDS = 0.2
WEBP_QUALITY = 85

HEADERS = {
    "User-Agent": "Mozilla/5.0 (compatible; poke-juegos-data-fetch/1.0)",
    "Accept": "image/png,image/*",
}


def download_bytes(url: str, retries: int = 3) -> bytes:
    request = urllib.request.Request(url, headers=HEADERS)
    last_error = None
    for attempt in range(retries):
        try:
            with urllib.request.urlopen(request, timeout=15) as response:
                return response.read()
        except (urllib.error.URLError, urllib.error.HTTPError) as error:
            last_error = error
            if attempt < retries - 1:
                time.sleep(1 * (attempt + 1))  # backoff simple: 1s, 2s, ...
    raise last_error


def convert_to_webp(png_bytes: bytes, output_path: Path) -> None:
    image = Image.open(io.BytesIO(png_bytes)).convert("RGBA")
    image.save(output_path, format="WEBP", quality=WEBP_QUALITY)


def sprite_file_name(pokemon_id: int) -> str:
    return f"pokemon_{pokemon_id}.webp"


def main() -> None:
    pokemon_list = json.loads(DETAILS_FILE.read_text())
    SPRITES_DIR.mkdir(parents=True, exist_ok=True)

    total = len(pokemon_list)
    print(f"Downloading and converting {total} sprites to WebP...")

    failed = []
    for i, pokemon in enumerate(pokemon_list, start=1):
        pokemon_id = pokemon["id"]
        artwork = pokemon["sprites"]["other"]["official-artwork"]
        source_url = artwork["front_default"]
        file_name = sprite_file_name(pokemon_id)
        output_path = SPRITES_DIR / file_name

        if source_url is None:
            print(f"  [{i}/{total}] {pokemon['name']}: no sprite, skipping")
            continue

        try:
            png_bytes = download_bytes(source_url)
            convert_to_webp(png_bytes, output_path)
        except (urllib.error.URLError, urllib.error.HTTPError, OSError) as error:
            print(f"  [{i}/{total}] ERROR with '{pokemon['name']}': {error}")
            failed.append(pokemon["name"])
            continue

        artwork["front_default"] = file_name
        print(f"  [{i}/{total}] {pokemon['name']} ok")
        time.sleep(REQUEST_DELAY_SECONDS)

    DETAILS_FILE.write_text(json.dumps(pokemon_list, indent=2, ensure_ascii=False) + "\n")

    ok_count = total - len(failed)
    print(f"\nDone: {SPRITES_DIR} ({ok_count}/{total} sprites)")
    if failed:
        print(f"Failed {len(failed)}: {', '.join(failed)}")


if __name__ == "__main__":
    main()
