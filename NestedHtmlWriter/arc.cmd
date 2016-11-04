mkdir wk
copy help\*.chm wk
copy NestedHtmlWriter\bin\Release\*.dll wk
del nestedhtmlwriter.zip
azip nestedhtmlwriter.zip wk
rem confirm to extract
azip -d nestedhtmlwriter.zip c:\delme
