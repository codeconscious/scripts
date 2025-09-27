#!/bin/bash
input_image="$1"

# Ensure proper locale settings
export LANG=ja_JP.UTF-8
export LC_ALL=ja_JP.UTF-8

exif_title=$(exiftool -s3 -Title "$input_image")
exif_description=$(exiftool -s3 -Description "$input_image")
exif_datetime=$(exiftool -s3 -d "%Y年%m月%d日 %H時%M分" -DateTimeOriginal "$input_image")

# Add text after `:-` to specify backup text if the value is missing.
metadata_text="${exif_title:-}　　｜　　${exif_description:-}　　｜　　${exif_datetime}"

magick "$input_image" \
  -bordercolor white \
  -border 20 \
  -gravity South \
  -background white \
  -splice 0x50 \
  -font KozMinPro \
  -font KozMinPro-Medium \
  -font "Hiragino Mincho Pro" \
  -font "YuKyokasho Yoko" \
  -font "Noto-Sans-CJK-JP" \
  -font "Source-Han-Sans-JP" \
  -font "Hiragino-Sans-GB" \
  -font "/Library/Fonts/Arial Unicode.ttf" \
  -pointsize 45 \
  -fill black \
  -annotate +0+10 "$metadata_text" \
  "${input_image}_annotated.jpg"
