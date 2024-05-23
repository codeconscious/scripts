#!/usr/bin/env bash

[[ ! -a $(which git) ]] && echo "Git is missing." && exit

DIRECTORY=$1
if [ ! -d "$DIRECTORY" ]; then
  echo "\"$DIRECTORY\" does not exist."
fi

# find "$DIRECTORY" -type d -exec echo "Hello, '{}'" \;
# find "$DIRECTORY" -name \*.fsx -ls
# find "$DIRECTORY" -not -path '*/.*' -exec echo "Hello, '{}'" \;
# find "$DIRECTORY" -not -path '*/.*'  -exec git --no-pager log -1 --pretty="format:%ci" {} \;

# Adapted from https://stackoverflow.com/a/58330257/11767771:
for f in $(git ls-files "$DIRECTORY")
do
    git --no-pager log --color -1 --date=short --pretty=format:'%C(cyan)%ai%Creset' -- "$f"
    # echo  " $f"; done | sort -r
    echo  " $f"
done
