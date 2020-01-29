content="["
for dir in `find . -type f`; do
    if [[ $dir != *".DS_Store"*  && $dir != *"generate_contents"* && $dir != *"tutorialSceneContents.json"* ]]; then
        path=${dir:2}
        content+="{\"file\": \"$path\", \"hash\": \"$path\"},"
    fi
done
content=${content%?}"]"
echo $content > tutorialSceneContents.json