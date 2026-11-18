#!/bin/sh
set -eu

# Stop on the first failure so the command cannot report a partial update as successful.
for context in ApplicationSecurityDbContext ApplicationDbContext; do
    case "$context" in
        ApplicationSecurityDbContext) project=Infrastructure/StepByStepCoding.Security ;;
        ApplicationDbContext) project=Infrastructure/StepByStepCoding.Persistence ;;
    esac

    echo "Applying migrations for $context..."

    /tools/dotnet-ef database update \
        --project "$project" \
        --startup-project API/StepByStepCoding.Api \
        --context "$context" \
        --configuration Release \
        --no-build
done

echo "Both databases are up to date."
