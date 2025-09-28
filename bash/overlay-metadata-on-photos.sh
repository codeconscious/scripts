#!/bin/bash
input_image="$1"
echo "▲ $input_image"

image_base_filename=$(basename -s .jpg "$input_image")
dir_name=$(dirname "$input_image")
parent_dir_name=$(dirname "$dir_name")

xmp_file="${parent_dir_name}/${image_base_filename}.ORF.xmp"

# Exit if no sidecar file found
if [[ ! -f "$xmp_file" ]]; then
    echo "No XMP sidecar file found at $xmp_file"
    exit 1
fi

echo "Sidebar file: $xmp_file"

# Locale settings
export LANG=ja_JP.UTF-8
export LC_ALL=ja_JP.UTF-8

title=$(exiftool -XMP-dc:Title -b "$xmp_file")
if [[ -n "$title" ]]; then
    echo "Title: $title"
else
    echo "No title found"
fi

description=$(exiftool -XMP-dc:Description -b "$xmp_file")
if [[ -n "$description" ]]; then
    echo "Description: $description"
else
    echo "No description found"
fi


datetime=$(exiftool -s3 -d "%Y年%m月%d日 %H時%M分" -DateTimeOriginal "$input_image")

output_photo="${input_image}_annotated.jpg"

magick "$input_image" \
    -bordercolor white \
    -border 40x80 \
    -gravity South \
    -font KozMinPro \
    -font KozMinPro-Medium \
    -font "Hiragino Mincho Pro" \
    -font "YuKyokasho Yoko" \
    -font "Noto-Sans-CJK-JP" \
    -font "Source-Han-Sans-JP" \
    -font "Hiragino-Sans-GB" \
    -font "/Library/Fonts/Arial Unicode.ttf" \
    -pointsize 55 \
    -fill black \
    -gravity Northwest \
    -annotate +40+4 "$title" \
    -gravity Northeast \
    -annotate +40+4 "$datetime" \
    -gravity South \
    -annotate +0+10 "$description" \
    "$output_photo"

echo "Annotated photo saved as $output_photo"
