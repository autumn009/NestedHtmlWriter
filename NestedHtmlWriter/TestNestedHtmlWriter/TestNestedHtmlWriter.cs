using System;
using System.IO;
using NUnit.Framework;
using NestedHtmlWriter;

namespace TestNestedHtmlWriter
{
	[TestFixture]
	public class TestNestedHtmlWriter
	{
		[SetUp]
		public void SetUp()
		{
			NhSetting.StrictNestChecking = true;
		}
		[Test]
		public void TestBody1()
		{
			StringWriter writer = new StringWriter();
			using( NhBody body = new NhBody( writer, null ) )
			{
			}
			Assertion.AssertEquals( "<body>\r\n</body>\r\n", writer.ToString() );
		}

		[Test]
		public void TestBodyAndP1()
		{
			StringWriter writer = new StringWriter();
			using( NhBody body = new NhBody( writer, null ) )
			{
				using( NhP p = body.CreateP() )
				{
					p.WriteText("test");
				}
			}
			Assertion.AssertEquals( "<body>\r\n<p>test</p>\r\n</body>\r\n", writer.ToString() );
		}

		[Test]
		[ExpectedException(typeof(NhException))]
		public void TestBodyAndP2()
		{
			StringWriter writer = new StringWriter();
			using( NhBody body = new NhBody( writer, null ) )
			{
				using( NhP p = body.CreateP() )
				{
					NhP p2 = body.CreateP();
				}
			}
		}

		[Test]
		public void TestBodyAndP3()
		{
			NhSetting.StrictNestChecking = false;
			StringWriter writer = new StringWriter();
			using( NhBody body = new NhBody( writer, null ) )
			{
				using( NhP p = body.CreateP() )
				{
					using( NhP p2 = body.CreateP() )
					{
					}
				}
			}
			// íçà”: Ç±ÇÃèÍçáÇÃåãâ ÇÕïsíËÇ≈Ç†ÇËÅAóéÇøÇ»Ç¢Ç≈Ç±Ç±Ç‹Ç≈óàÇÍÇŒçáäi
			Assertion.Assert( true );
		}

		[Test]
		public void TestDocument1()
		{
			const string r1 = "<?xml version=\"1.0\"?>\r\n"
					  + "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"\r\n"
					  + "\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n"
					  + "<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n"
					  + "<head>\r\n"
					  + "<title>test title</title>\r\n"
					  + "</head>\r\n"
					  + "<body>\r\n"
					  + "<p>test para</p>\r\n"
					  + "</body>\r\n"
					  + "</html>\r\n";

			StringWriter writer = new StringWriter();
			NhDocument doc = new NhDocument( writer );
			using( NhHtml html = doc.CreateHtml() )
			{
				using( NhHead head = html.CreateHead() )
				{
					head.WriteTitle("test title");
				}
				using( NhBody body = html.CreateBody() )
				{
					using( NhP p = body.CreateP() )
					{
						p.WriteText("test para");
					}
				}
			}
			//System.Diagnostics.Trace.WriteLine(r1);
			//System.Diagnostics.Trace.WriteLine(writer.ToString());
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}

		[Test]
		public void TestQuickNestedDocument1()
		{
			const string r1 = "<?xml version=\"1.0\"?>\r\n"
					  + "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"\r\n"
					  + "\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n"
					  + "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"ja-JP\">\r\n"
					  + "<head>\r\n"
					  + "<title>test title</title>\r\n"
					  + "<link href=\"default.css\" type=\"text/css\" rel=\"stylesheet\" />\r\n"
					  + "</head>\r\n"
					  + "<body>\r\n"
					  + "<p>test para</p>\r\n"
					  + "</body>\r\n"
					  + "</html>\r\n";

			StringWriter writer = new StringWriter();
			using( NhQuickDocument doc = new NhQuickDocument( writer, 
				"test title", "default.css", "text/css", "ja-JP", NhDocumentType.Xhtml11 ) )
			{
				doc.B.WritePText("test para");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}

		[Test]
		public void TestQuickNestedDocument2()
		{
			const string r1 = "<?xml version=\"1.0\"?>\r\n"
					  + "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"\r\n"
					  + "\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">\r\n"
					  + "<html xmlns=\"http://www.w3.org/1999/xhtml\" lang=\"ja-JP\" xml:lang=\"ja-JP\">\r\n"
					  + "<head>\r\n"
					  + "<title>test title</title>\r\n"
					  + "<link href=\"default.css\" type=\"text/css\" rel=\"stylesheet\" />\r\n"
					  + "</head>\r\n"
					  + "<body>\r\n"
					  + "<p>test para</p>\r\n"
					  + "</body>\r\n"
					  + "</html>\r\n";

			StringWriter writer = new StringWriter();
			using( NhQuickDocument doc = new NhQuickDocument( writer, 
					   "test title", "default.css", "text/css", "ja-JP", NhDocumentType.Xhtml10Strict ) )
			{
				doc.B.WritePText("test para");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}

		[Test]
		public void TestQuickNestedDocument3()
		{
			const string r1 = "<?xml version=\"1.0\"?>\r\n"
					  + "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\r\n"
					  + "\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n"
					  + "<html xmlns=\"http://www.w3.org/1999/xhtml\" lang=\"ja-JP\" xml:lang=\"ja-JP\">\r\n"
					  + "<head>\r\n"
					  + "<title>test title</title>\r\n"
					  + "<link href=\"default.css\" type=\"text/css\" rel=\"stylesheet\" />\r\n"
					  + "</head>\r\n"
					  + "<body>\r\n"
					  + "<p>test para</p>\r\n"
					  + "</body>\r\n"
					  + "</html>\r\n";

			StringWriter writer = new StringWriter();
			using( NhQuickDocument doc = new NhQuickDocument( writer, 
					   "test title", "default.css", "text/css", "ja-JP", NhDocumentType.Xhtml10Transitional ) )
			{
				doc.B.WritePText("test para");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}

		[Test]
		public void TestQuickNestedDocument4()
		{
			const string r1 = "<?xml version=\"1.0\"?>\r\n"
					  + "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"\r\n"
					  + "\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">\r\n"
					  + "<html xmlns=\"http://www.w3.org/1999/xhtml\" lang=\"ja-JP\" xml:lang=\"ja-JP\">\r\n"
					  + "<head>\r\n"
					  + "<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\" />\r\n"
					  + "<title>test title</title>\r\n"
					  + "<link href=\"default.css\" type=\"text/css\" rel=\"stylesheet\" />\r\n"
					  + "</head>\r\n"
					  + "<body>\r\n"
					  + "<p>test para</p>\r\n"
					  + "</body>\r\n"
					  + "</html>\r\n";

			StringWriter writer = new StringWriter();
			using( NhQuickDocument doc = new NhQuickDocument( writer, 
					   "test title", "default.css", "text/css", "ja-JP", NhDocumentType.Xhtml10Transitional, "utf-8" ) )
			{
				doc.B.WritePText("test para");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}

		[Test]
		public void TestImg1()
		{
			const string r1 = "<?xml version=\"1.0\"?>\r\n"
					  + "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"\r\n"
					  + "\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">\r\n"
					  + "<html xmlns=\"http://www.w3.org/1999/xhtml\" xml:lang=\"ja-JP\">\r\n"
					  + "<head>\r\n"
					  + "<title>test title</title>\r\n"
					  + "<link href=\"default.css\" type=\"text/css\" rel=\"stylesheet\" />\r\n"
					  + "</head>\r\n"
					  + "<body>\r\n"
					  + "<p>NhA<img src=\"test.png\" />B</p>\r\n"
					  + "</body>\r\n"
					  + "</html>\r\n";

			StringWriter writer = new StringWriter();
			using( NhQuickDocument doc = new NhQuickDocument( writer, 
				"test title", "default.css", "text/css", "ja-JP", NhDocumentType.Xhtml11 ) )
			{
				using( NhP p = doc.B.CreateP() )
				{
					p.WriteText("NhA");
					using( NhImg img = p.CreateImg() )
					{
						img.WriteAttribute( "src", "test.png" );
					}
					p.WriteText("B");
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}	

		[Test]
		public void TestStrong1()
		{
			const string r1 = "<p><strong><strong>test</strong></strong></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				using( NhStrong strong1 = p.CreateStrong() )
				{
					using( NhStrong strong2 = strong1.CreateStrong() )
					{
						strong2.WriteText("test");
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}

		[Test]
		public void TestEm1()
		{
			const string r1 = "<p><em><em>test</em></em></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				using( NhEm em1 = p.CreateEm() )
				{
					using( NhEm em2 = em1.CreateEm() )
					{
						em2.WriteText("test");
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}

		[Test]
		public void TestHx1()
		{
			const string r1 = "<h2>test</h2>\r\n";

			StringWriter writer = new StringWriter();
			using( NhHx hx = new NhHx( 2, writer, null ) )
			{
				hx.WriteText("test");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestSpan1()
		{
			const string r1 = "<p><span><span>test</span></span></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				using( NhSpan Span1 = p.CreateSpan() )
				{
					using( NhSpan Span2 = Span1.CreateSpan() )
					{
						Span2.WriteText("test");
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestDiv1()
		{
			const string r1 = "<div>\r\n<p>test</p>\r\n</div>\r\n";

			StringWriter writer = new StringWriter();
			using( NhDiv div = new NhDiv( writer, null ) )
			{
				div.WritePText("test");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestA1()
		{
			const string r1 = "<p><a>test</a></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				using( NhA a = p.CreateA() )
				{
					a.WriteText("test");
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestOl()
		{
			const string r1 = "<ol>\r\n<li><strong>test</strong></li>\r\n</ol>\r\n";

			StringWriter writer = new StringWriter();
			using( NhOl ol = new NhOl( writer, null ) )
			{
				using( NhLiInline li = ol.CreateLiInline() )
				{
					using( NhStrong strong = li.CreateStrong() )
					{
						strong.WriteText("test");
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}

#if false
		[Test]
		public void TestOldLi()
		{
			const string r1 = "<ul>\r\n<li><strong>test</strong></li>\r\n</ul>\r\n";

			StringWriter writer = new StringWriter();
			using( NhUl ul = new NhUl( writer, null ) )
			{
				using( NhLi li = ul.CreateLi() )
				{
					using( NhStrong strong = li.CreateStrong() )
					{
						strong.WriteText("test");
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
#endif

		[Test]
		public void TestUlAndLiInline()
		{
			const string r1 = "<ul>\r\n<li><strong>test</strong></li>\r\n</ul>\r\n";

			StringWriter writer = new StringWriter();
			using( NhUl ul = new NhUl( writer, null ) )
			{
				using( NhLiInline li = ul.CreateLiInline() )
				{
					using( NhStrong strong = li.CreateStrong() )
					{
						strong.WriteText("test");
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestLiBlock()
		{
			const string r1 = "<ul>\r\n<li>\r\n<p>test</p>\r\n</li>\r\n</ul>\r\n";

			StringWriter writer = new StringWriter();
			using( NhUl ul = new NhUl( writer, null ) )
			{
				using( NhLiBlock li = ul.CreateLiBlock() )
				{
					using( NhP p = li.CreateP() )
					{
						p.WriteText("test");
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}

		[Test]
		public void TestTable()
		{
			const string r1 = "<table>\r\n"
					  + "<caption>cap</caption>\r\n"
					  + "<tr>\r\n<th>test</th>\r\n<th>\r\n<p>test</p>\r\n</th>\r\n"
					  + "<td>test</td>\r\n<td>\r\n<p>test</p>\r\n</td>\r\n"
					  + "</tr>\r\n</table>\r\n";

			StringWriter writer = new StringWriter();
			using( NhTable table = new NhTable( writer, null ) )
			{
				using( NhCaption caption = table.CreateCaption() )
				{
					caption.WriteText("cap");
				}
				using( NhTr tr = table.CreateTr() )
				{
					using( NhThInline th1 = tr.CreateThInline() )
					{
						th1.WriteText("test");
					}
					using( NhThBlock th2 = tr.CreateThBlock() )
					{
						th2.WritePText("test");
					}
					using( NhTdInline td1 = tr.CreateTdInline() )
					{
						td1.WriteText("test");
					}
					using( NhTdBlock td2 = tr.CreateTdBlock() )
					{
						td2.WritePText("test");
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestBr()
		{
			const string r1 = "<p><br /></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				using( NhBr br = p.CreateBr() )
				{
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteHxText()
		{
			const string r1 = "<body>\r\n<h3>test</h3>\r\n</body>\r\n";

			StringWriter writer = new StringWriter();
			using( NhBody body = new NhBody( writer, null ) )
			{
				body.WriteHxText( 3, "test" );
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteLiText1()
		{
			const string r1 = "<ol>\r\n<li>test</li>\r\n</ol>\r\n";

			StringWriter writer = new StringWriter();
			using( NhOl ol = new NhOl( writer, null ) )
			{
				ol.WriteLiText( "test" );
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteLiText2()
		{
			const string r1 = "<ul>\r\n<li>test</li>\r\n</ul>\r\n";

			StringWriter writer = new StringWriter();
			using( NhUl ul = new NhUl( writer, null ) )
			{
				ul.WriteLiText( "test" );
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteA1()
		{
			const string r1 = "<p><a href=\"a&amp;.txt\">test&amp;</a></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				p.WriteAText("a&.txt","test&");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteClassAttr()
		{
			const string r1 = "<p class=\"test\">text</p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				p.WriteClassAttr("test");
				p.WriteText("text");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteIDAttr()
		{
			const string r1 = "<p id=\"test\">text</p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				p.WriteIDAttr("test");
				p.WriteText("text");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteImg1()
		{
			const string r1 = "<p><img src=\"http://a.png?a=b&amp;c=d\" alt=\"test\" width=\"123\" height=\"456\" /></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				p.WriteImg( "http://a.png?a=b&c=d", "test", 123, 456 );
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteImg2()
		{
			const string r1 = "<p><img src=\"http://a.png?a=b&amp;c=d\" alt=\"test\" /></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				p.WriteImg( "http://a.png?a=b&c=d", "test" );
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteBr()
		{
			const string r1 = "<p><br /></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				p.WriteBr();
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestWriteRawString()
		{
			const string r1 = "<p>abc&amp;<br />def</p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				p.WriteRawString("abc&amp;<br />def");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestCreateForm1()
		{
			const string r1 = "<form action=\"abc.txt\" method=\"get\">\r\n</form>\r\n";

			StringWriter writer = new StringWriter();
			using( NhForm form = new NhForm( writer, null, "abc.txt" ) )
			{
				form.WriteGetMethod();
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestCreateForm2()
		{
			const string r1 = "<form action=\"abc.txt\" method=\"post\">\r\n</form>\r\n";

			StringWriter writer = new StringWriter();
			using( NhForm form = new NhForm( writer, null, "abc.txt" ) )
			{
				form.WritePostMethod();
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestCreateForm3()
		{
			const string r1 = "<div>\r\n<form action=\"abc.txt\" method=\"post\">\r\n</form>\r\n</div>\r\n";

			StringWriter writer = new StringWriter();
			using( NhDiv div = new NhDiv(writer,null) )
			{
				using( NhForm form = div.CreateForm("abc.txt") )
				{
					form.WritePostMethod();
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestCreateInput1()
		{
			const string r1 = "<form action=\"abc.txt\">\r\n<p><input /></p>\r\n</form>\r\n";

			StringWriter writer = new StringWriter();
			using( NhForm form = new NhForm( writer, null, "abc.txt" ) )
			{
				using( NhP p = form.CreateP() )
				{
					using( NhInput input = p.CreateInput() )
					{
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestCreateLabel1()
		{
			const string r1 = "<p><label>test</label></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP(writer,null) )
			{
				using( NhLabel label = p.CreateLabel() )
				{
					label.WriteText("test");
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestCreateSelect1()
		{
			const string r1 = "<p><select><option>test</option></select></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP(writer,null) )
			{
				using( NhSelect select = p.CreateSelect() )
				{
					using( NhOption option = select.CreateOption() )
					{
						option.WriteText("test");
					}
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestCreateTextArea1()
		{
			const string r1 = "<p><textarea cols=\"12\" rows=\"34\">test</textarea></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP(writer,null) )
			{
				using( NhTextArea select = p.CreateTextArea(12,34) )
				{
					select.WriteText("test");
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestQ1()
		{
			const string r1 = "<p><q>test</q></p>\r\n";

			StringWriter writer = new StringWriter();
			using( NhP p = new NhP( writer, null ) )
			{
				using( NhQ q1 = p.CreateQ() )
				{
					q1.WriteText("test");
				}
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
		[Test]
		public void TestBlockQuote1()
		{
			const string r1 = "<blockquote>\r\n<p>test</p>\r\n</blockquote>\r\n";

			StringWriter writer = new StringWriter();
			using( NhBlockQuote div = new NhBlockQuote( writer, null ) )
			{
				div.WritePText("test");
			}
			string s1 = writer.ToString();
			Assertion.AssertEquals( r1, s1 );
		}
	}
}
