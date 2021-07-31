#!/bin/bash

rsync -va --delete --exclude .git --exclude node_modules "$HOME/code/explorer/" $HOME/code/kernel