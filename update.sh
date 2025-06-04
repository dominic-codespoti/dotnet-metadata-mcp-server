#!/bin/bash

# Usage: ./scripts/publish-to-dockerhub.sh <dockerhub-username> <version>
# Example: ./scripts/publish-to-dockerhub.sh myusername v1.2.3

set -e

if [ $# -ne 2 ]; then
  echo "Usage: $0 <dockerhub-username> <version>"
  exit 1
fi

DOCKERHUB_USER="$1"
VERSION="$2"
IMAGE_NAME="dotnetmetadatamcpserver"

# Build the Docker image with version and latest tags
docker build -t "$DOCKERHUB_USER/$IMAGE_NAME:latest" -t "$DOCKERHUB_USER/$IMAGE_NAME:$VERSION" .

# Push both tags to Docker Hub
docker push "$DOCKERHUB_USER/$IMAGE_NAME:latest"
docker push "$DOCKERHUB_USER/$IMAGE_NAME:$VERSION"

echo "Docker images pushed:"
echo "  $DOCKERHUB_USER/$IMAGE_NAME:latest"
echo "  $DOCKERHUB_USER/$IMAGE_NAME:$VERSION"

# Last 1.0.0