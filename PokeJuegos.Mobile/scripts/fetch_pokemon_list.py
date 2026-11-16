#!/usr/bin/env python3
"""Generates Resources/Raw/pokemon_list.json with the first 3 generations of Pokemon
(national dex #1 to #386), in the same shape the PokeAPI list endpoint
returns (a "results" array of {name, url}), so fetch_pokemon_details.py can
consume it without any changes.

Run once, by hand, before fetch_pokemon_details.py. Only requires Python 3
(stdlib, no external dependencies).

Usage:
    python3 scripts/fetch_pokemon_list.py
"""
import json
import urllib.error
import urllib.request
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
OUTPUT_FILE = ROOT / "Resources" / "Raw" / "pokemon_list.json"
API_URL = "https://pokeapi.co/api/v2/pokemon?limit=386"

HEADERS = {
    "User-Agent": "Mozilla/5.0 (compatible; poke-juegos-data-fetch/1.0)",
    "Accept": "application/json",
}


def main() -> None:
    request = urllib.request.Request(API_URL, headers=HEADERS)
    try:
        with urllib.request.urlopen(request, timeout=15) as response:
            data = json.loads(response.read())
    except (urllib.error.URLError, urllib.error.HTTPError) as error:
        print(f"ERROR fetching pokemon list: {error}")
        raise SystemExit(1)

    OUTPUT_FILE.parent.mkdir(parents=True, exist_ok=True)
    OUTPUT_FILE.write_text(json.dumps(data, indent=2, ensure_ascii=False) + "\n")

    print(f"Done: {OUTPUT_FILE} ({len(data['results'])} pokemon, gen I-III)")


if __name__ == "__main__":
    main()
