#!/bin/bash
# Uses AFConvert (https://ss64.com/mac/afconvert.html) to rip and convert audio files.
for file in *.aiff; do
  filename="$(basename "${file%.aiff}").m4a"
  echo "Converting: $filename"
  afconvert -f m4af -d aac -b 128000 "$file" ~/Downloads/"$filename"
  echo "✓ Completed: $filename"
done
