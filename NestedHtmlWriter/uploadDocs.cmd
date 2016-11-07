rem use after build document files
pushd C:\xgit\NestedHtmlWriterDoc\NestedHtmlWriter.wiki
xcopy /s/e/y C:\xgit\NestedHtmlWriter\NestedHtmlWriter\NestedHtmlWriterDocs\Help C:\xgit\NestedHtmlWriterDoc\NestedHtmlWriter.wiki
git add .
git commit -m "modefy"
git push origin master
popd
pause
