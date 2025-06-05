#!/bin/bash

# Update permissions
find "$1" -type f -exec chmod 666 -- {} +
find "$1" -type d -exec chmod 777 -- {} +
