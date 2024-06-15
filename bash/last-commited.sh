#!/usr/bin/env bash

# Passed a single directory, lists the dates each file in that directory were last committed in git.
# TODO: Check for no directory passed in.

[[ ! -a $(which git) ]] && echo "Git is missing." && exit

DIRECTORY=$1
if [ ! -d "$DIRECTORY" ]; then
  echo "\"$DIRECTORY\" does not exist."
fi

# Adapted from https://stackoverflow.com/a/58330257/11767771:
for f in $(git ls-files "$DIRECTORY")
do
    git --no-pager log --color -1 --date=short --pretty=format:'%C(cyan)%ai%Creset' -- "$f"
    # echo  " $f"; done | sort -r
    echo  ", $f"
done
