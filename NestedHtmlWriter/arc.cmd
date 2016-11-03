"\Program Files\NDoc\NDocConsole.exe" -documenter=MSDN -project=NestedHtmlWriter\NestedHtmlWriter.ndoc NestedHtmlWriter\bin\release\NestedHtmlWriter.xml
del NestedHtmlWriter.lzh
tlha a NestedHtmlWriter.lzh NestedHtmlWriter\*.cs -d
tlha a NestedHtmlWriter.lzh NestedHtmlWriter\*.csproj -d
tlha a NestedHtmlWriter.lzh NestedHtmlWriter\NestedHtmlWriter.ndoc -d
tlha a NestedHtmlWriter.lzh TestNestedHtmlWriter\*.cs -d
tlha a NestedHtmlWriter.lzh TestNestedHtmlWriter\*.csproj -d
tlha a NestedHtmlWriter.lzh NestedHtmlWriter.sln
tlha a NestedHtmlWriter.lzh TestNestedHtmlWriter\bin\Release\NestedHtmlWriter.dll
tlha a NestedHtmlWriter.lzh TestNestedHtmlWriter\bin\Release\NestedHtmlWriter.xml
tlha a NestedHtmlWriter.lzh NestedHtmlWriter.txt
tlha a NestedHtmlWriter.lzh doc -d
tlha a NestedHtmlWriter.lzh NestedHtmlWriter.sln
