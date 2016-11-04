using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using NestedHtmlWriter;

namespace SimpleSample001
{
    class Program
    {
        static void Main(string[] args)
        {
            const string title = "SimpleSample001 for NestedHtmlWriter";
            const string filename = "test.html";
            using (var writer = new StreamWriter(filename))
            {
                // example for quick create document
                using (var doc = new NhQuickDocument(writer, title, null, null, "us-en", NhDocumentType.Html5))
                {
                    // example for h1 element
                    doc.B.WriteHxText(1, title);
                    // exaple for p erelemnt
                    doc.B.WritePText("Hello NestedHtmlWriter World");
                    // example for table element
                    using (var table = doc.B.CreateTable())
                    {
                        using (var tr = table.CreateTr())
                        {
                            using (var th = tr.CreateThInline())
                            {
                                // example for simple text
                                th.WriteText("NestedHtmlWriter");
                            }
                            using (var th = tr.CreateThInline())
                            {
                                th.WriteText(" in github");
                            }
                        }
                        using (var tr = table.CreateTr())
                        {
                            // example for two type of strong text
                            using (var td = tr.CreateTdInline())
                            {
                                td.WriteRawString("<strong>BOLD STYLE</strong>");
                            }
                            using (var td = tr.CreateTdInline())
                            {
                                using (var strong = td.CreateStrong())
                                {
                                    strong.WriteText("BOLD STYLE");
                                }
                            }
                        }
                        using (var tr = table.CreateTr())
                        {
                            // example for two type of hyper-linked text
                            using (var td = tr.CreateTdInline())
                            {
                                using (var a = td.CreateA())
                                {
                                    a.WriteAttribute("href", "https://github.com/autumn009/NestedHtmlWriter");
                                    a.WriteText("Let's Go!");
                                }
                            }
                            using (var td = tr.CreateTdInline())
                            {
                                td.WriteAText("https://github.com/autumn009/NestedHtmlWriter",
                                    "Let's Go!");
                            }
                        }
                    }
                }
            }
            Process.Start(filename);
        }
    }
}
