rem @echo off
if "%1" == "" goto usage
\w\uty\OSDM\mktextdoc NestedHtmlWriter%1.osdf NestedHtmlWriter*.osdf NestedHtmlWriter.txt
goto end
:usage
echo gendoc 001�̂悤�Ɏg��
echo ���̏ꍇestedHtmlWriter001.osdf����������
:end
