rem @echo off
if "%1" == "" goto usage
\w\uty\OSDM\mktextdoc NestedHtmlWriter%1.osdf NestedHtmlWriter*.osdf NestedHtmlWriter.txt
goto end
:usage
echo gendoc 001のように使う
echo この場合estedHtmlWriter001.osdfを処理する
:end
