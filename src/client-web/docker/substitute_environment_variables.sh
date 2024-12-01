#!/bin/sh

ROOT_DIR=/app

if [ -z "$ARG_VARIABLE" ]; then
  for file in $ROOT_DIR/.env;
  do
    sed -i 's#^VUE_APP_BACKEND_API_URL.*#VUE_APP_BACKEND_API_URL='$VUE_APP_BACKEND_API_URL'#' $file
  done
fi
