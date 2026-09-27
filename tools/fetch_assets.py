"""Fetch game assets for the PvZ WP8 decomp port.

Downloads the game bundle from Google Drive, extracts only the runtime
assets (Content/, resources.xml, todresources.xml, LawnStrings_*.txt)
and converts the WMA music to OGG Vorbis in place (MonoGame/DesktopGL
only decodes OGG for songs; the .xnb files just store the file name,
so keeping the .wma filename with OGG data inside just works).

Usage:
    python3 tools/fetch_assets.py [--id FILE_ID] [--dest DIR]

Requires ffmpeg on PATH for the music conversion step.
"""
import argparse
import http.cookiejar
import os
import re
import shutil
import subprocess
import sys
import urllib.request
import zipfile

DEFAULT_FILE_ID = "1azLfawLxQZaKmmD5ZoVLvrAQzC3mt0GO"

WANTED_PREFIXES = ("Content/",)
WANTED_FILES = {
    "resources.xml",
    "todresources.xml",
    "LawnStrings_de.txt",
    "LawnStrings_en.txt",
    "LawnStrings_es.txt",
    "LawnStrings_fr.txt",
    "LawnStrings_it.txt",
}


def download_drive_file(file_id, dest_path):
    cj = http.cookiejar.CookieJar()
    opener = urllib.request.build_opener(urllib.request.HTTPCookieProcessor(cj))

    def fetch(url):
        req = urllib.request.Request(
            url, headers={"User-Agent": "Mozilla/5.0"})
        return opener.open(req)

    url = f"https://drive.google.com/uc?export=download&id={file_id}"
    resp = fetch(url)
    ctype = resp.headers.get("Content-Type", "")
    if "text/html" in ctype:
        # Large-file confirmation page: find the confirm token and retry.
        page = resp.read().decode("utf-8", "replace")
        m = re.search(r'"confirm=([^"&]+)', page) or re.search(
            r'name="confirm"\s+value="([^"]+)"', page)
        uuid = re.search(r'"uuid","([^"]+)"', page)
        if m:
            url = (f"https://drive.google.com/uc?export=download"
                   f"&confirm={m.group(1)}&id={file_id}")
        elif uuid:
            url = (f"https://drive.google.com/uc?export=download"
                   f"&confirm=t&uuid={uuid.group(1)}&id={file_id}")
        else:
            raise RuntimeError("could not find Drive confirm token")
        resp = fetch(url)
    total = 0
    with open(dest_path, "wb") as f:
        while True:
            chunk = resp.read(1024 * 1024)
            if not chunk:
                break
            f.write(chunk)
            total += len(chunk)
    print(f"downloaded {total / 1e6:.1f} MB -> {dest_path}")
    with open(dest_path, "rb") as f:
        if f.read(2) != b"PK":
            raise RuntimeError("download is not a zip file")


def extract_assets(zip_path, dest_dir):
    count = 0
    with zipfile.ZipFile(zip_path) as z:
        for info in z.infolist():
            name = info.filename
            if name.endswith("/"):
                continue
            keep = name in WANTED_FILES or name.startswith(WANTED_PREFIXES)
            if not keep:
                continue
            target = os.path.join(dest_dir, *name.split("/"))
            os.makedirs(os.path.dirname(target), exist_ok=True)
            with z.open(info) as src, open(target, "wb") as dst:
                shutil.copyfileobj(src, dst)
            count += 1
    print(f"extracted {count} asset files -> {dest_dir}")


def pick_vorbis_encoder(ffmpeg):
    try:
        out = subprocess.run(
            [ffmpeg, "-hide_banner", "-encoders"],
            capture_output=True, text=True).stdout
    except Exception:
        return "libvorbis"
    if "libvorbis" in out:
        return "libvorbis"
    return "vorbis"


def convert_music(dest_dir):
    ffmpeg = shutil.which("ffmpeg")
    if ffmpeg is None:
        raise RuntimeError("ffmpeg not found on PATH")
    encoder = pick_vorbis_encoder(ffmpeg)
    print(f"using audio encoder: {encoder}")
    music = os.path.join(dest_dir, "Content", "music")
    wmas = sorted(f for f in os.listdir(music) if f.endswith(".wma"))
    for wma in wmas:
        src = os.path.join(music, wma)
        with open(src, "rb") as f:
            if f.read(4) == b"OggS":
                print(f"  skip {wma} (already OGG)")
                continue
        tmp = src + ".ogg"
        r = subprocess.run(
            [ffmpeg, "-y", "-v", "error", "-i", src,
             "-c:a", encoder, "-q:a", "4", tmp])
        if r.returncode != 0:
            raise RuntimeError(f"ffmpeg failed on {wma}")
        os.replace(tmp, src)
        print(f"  converted {wma}")


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--id", default=DEFAULT_FILE_ID)
    ap.add_argument("--dest", default=".")
    args = ap.parse_args()

    tmp_zip = os.path.join(args.dest, ".assets_download.zip")
    os.makedirs(args.dest, exist_ok=True)
    download_drive_file(args.id, tmp_zip)
    try:
        extract_assets(tmp_zip, args.dest)
    finally:
        if os.path.exists(tmp_zip):
            os.remove(tmp_zip)
    convert_music(args.dest)
    print("assets ready")


if __name__ == "__main__":
    sys.exit(main())
