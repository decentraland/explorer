#!/bin/bash
set -e

add_line_prefix()
{
  echo "$(sed 's/^/- /' <<< "${1}")"
}

echo "## Kernel changelog:"
KERNEL_CHANGELOG=$(git log --oneline origin/release..HEAD --no-merges --pretty=%s)

add_line_prefix "${KERNEL_CHANGELOG}"

echo " "
echo "## Unity renderer changelog:"

HEAD_HASH=$(git rev-parse HEAD)
LAST_RELEASE_HASH=$(git rev-parse origin/release)

HEAD_PACKAGE=$(git show HEAD:kernel/package.json)
LAST_RELEASE_PACKAGE=$(git show origin/release:kernel/package.json)

HEAD_RENDERER_VERSION=$(jq '."devDependencies"."@dcl/unity-renderer"' <<<"$HEAD_PACKAGE" | tr -dc '0-9.')
LAST_RELEASE_RENDERER_VERSION=$(jq '."devDependencies"."@dcl/unity-renderer"' <<<"$LAST_RELEASE_PACKAGE" | tr -dc '0-9.')

HEAD_RENDERER_HASH=$(npm view @dcl/unity-renderer@${HEAD_RENDERER_VERSION} commit)
LAST_RELEASE_RENDERER_HASH=$(npm view @dcl/unity-renderer@${LAST_RELEASE_RENDERER_VERSION} commit)

git remote add renderer https://github.com/decentraland/unity-renderer.git
git fetch renderer --quiet

RENDERER_CHANGELOG=$(git log renderer/master --oneline ${LAST_RELEASE_RENDERER_HASH}..${HEAD_RENDERER_HASH} --no-merges --pretty=%s)

RENDERER_CHANGELOG=$(add_line_prefix "${RENDERER_CHANGELOG}")

echo "${RENDERER_CHANGELOG// \(#/ \(https://github.com/decentraland/unity-renderer/pull/}"

git remote remove renderer