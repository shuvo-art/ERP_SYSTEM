#!/bin/bash
# scripts/validate_all.sh
# Usage: ./scripts/validate_all.sh
# Purpose: Validates Terraform configuration in all environment directories

set -e

DIRECTORIES=("live/dev" "live/staging" "live/prod" "live/common")

echo "Starting Terraform Validation..."

for dir in "${DIRECTORIES[@]}"; do
    echo "------------------------------------------------"
    echo "Checking directory: $dir"
    if [ -d "$dir" ]; then
        cd "$dir"
        
        # Initialize backend (ignore errors if backend not reachable, just validation needed)
        echo "Initializing..."
        terraform init -backend=false
        
        echo "Validating..."
        terraform validate
        
        echo "Formatting Check..."
        terraform fmt -check
        
        cd - > /dev/null
        echo "OK: $dir"
    else
        echo "WARNING: Directory $dir not found"
    fi
done

echo "------------------------------------------------"
echo "All validations complete."
