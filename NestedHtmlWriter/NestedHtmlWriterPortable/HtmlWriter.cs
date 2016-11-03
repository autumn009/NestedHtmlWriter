using System;
using System.IO;

namespace NestedHtmlWriter
{
	/// <summary>
	/// 文書の種類を示します
	/// </summary>
	public enum NhDocumentType
	{
		/// <summary>
		/// XHTML 1.1を示します
		/// </summary>
		Xhtml11,
		/// <summary>
		/// XHTML 1.0 Strictを示します
		/// </summary>
		Xhtml10Strict,
		/// <summary>
		/// XHTML 1.0 Transitionalを示します
		/// </summary>
		Xhtml10Transitional
	}

	/// <summary>
	/// NestedHtmlWriterのグローバルな設定を保持します。
	/// </summary>
	public class NhSetting
	{
		/// <summary>
		/// 厳密なチェックを行うかどうかを指定します。デフォルトは行います。
		/// </summary>
		/// <remarks>
		/// 厳密なチェックを行うかどうかを指定します。デフォルトは行います。
		/// 厳密なチェックを行うと、多くの正しくない出力でNhException例外を発生させることができ、安全です。
		/// しかし、速度が重要な場合にはfalseとしてチェックを省くことができます。
		/// </remarks>
		public static bool StrictNestChecking = true;
	}
	/// <summary>
	/// 不正な組み合わせの出力を行おうとしている状況で投げられる例外です。
	/// </summary>
	public class NhException : Exception
	{
		/// <summary>
		/// NhExceptionクラスのコンストラクタです。
		/// </summary>
		public NhException()
		{
		}
		/// <summary>
		/// NhExceptionクラスのコンストラクタです。
		/// </summary>
		public NhException(string msg) : base(msg)
		{
		}
		/// <summary>
		/// NhExceptionクラスのコンストラクタです。
		/// </summary>
		public NhException(string msg, Exception ex) : base(msg,ex)
		{
		}
	}
	/// <summary>
	/// NestedHtmlWriterの全ての出力クラスの基底となるクラスです。
	/// </summary>
	public class NhBase
	{
		/// <summary>
		/// 出力に使用するライターです。
		/// </summary>
		protected TextWriter writer;
		/// <summary>
		/// 親の要素クラスを保持します。
		/// </summary>
		protected NhBase parent;
		/// <summary>
		/// ロックされているか調べて、例外を投げます。
		/// </summary>
		/// <remarks>
		/// 厳密なチェックを行わない場合は呼び出してはいけません。
		/// </remarks>
		protected void checkLock()
		{
			if( lockFlag )
			{
				throw new NhException("NestedHtmlWriterアイテムはロックされています。");
			}
		}
		/// <summary>
		/// NhBaseクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhBase( TextWriter writer, NhBase parent )
		{
			this.writer = writer;
			this.parent = parent;
		}
		/// <summary>
		/// 一切の加工を行わず文字列を出力します。
		/// 既にXMLのフラグメント形式になったデータを出力するために使用します。
		/// 利用は推奨しません。
		/// </summary>
		/// <param name="rawString">既にXMLのフラグメント形式になった文字列</param>
		public virtual void WriteRawString( string rawString )
		{
			writer.Write(rawString);
		}
		/// <summary>
		/// 任意の属性を出力します。
		/// </summary>
		/// <param name="name">属性の名前。名前空間使用時には、接頭辞:ローカル名の形式。</param>
		/// <param name="val">属性の値</param>
		public void WriteAttribute( string name, string val )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			writer.Write(' ');
			writer.Write(name);
			writer.Write("=\"");
			writer.Write(NhUtil.QuoteText(val));
			writer.Write("\"");
		}
		/// <summary>
		/// class属性を出力します。
		/// </summary>
		/// <param name="val">属性の値</param>
		public void WriteClassAttr( string val )
		{
			WriteAttribute( "class", val );
		}
		/// <summary>
		/// id属性を出力します。
		/// </summary>
		/// <param name="val">属性の値</param>
		public void WriteIDAttr( string val )
		{
			WriteAttribute( "id", val );
		}
		private bool lockFlag = false;
		internal void Lock()
		{
			if( lockFlag )
			{
				throw new NhException("既にロックされているNestedHtmlWriterのアイテムをロックしようとしました。");
			}
			lockFlag = true;
		}
		internal void Unlock()
		{
			if( !lockFlag )
			{
				throw new NhException("ロックされていないNestedHtmlWriterのアイテムをロック解除しようとしました。");
			}
			lockFlag = false;
		}
	}
	/// <summary>
	/// 空要素を表現する汎用クラスです。継承して使います。
	/// </summary>
	public class NhEmpty : NhBase, IDisposable
	{
		private string tagName;
		/// <summary>
		/// NhEmptyクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="tagName">タグ名。名前空間使用時には、接頭辞:ローカル名の形式</param>
		/// <param name="parent">親の要素クラス</param>
		public NhEmpty( TextWriter writer, string tagName, NhBase parent ) : base( writer, parent )
		{
			if( NhSetting.StrictNestChecking && parent != null ) parent.Lock();
			this.tagName = tagName;
			writer.Write('<');
			writer.Write(tagName);
		}
		/// <summary>
		/// 確保した資源を解放します。
		/// </summary>
		public void Dispose()
		{
			writer.Write(" />");
			if( !(parent is NhTextAvailable) )
			{
				writer.Write("\r\n");
			}
			if( NhSetting.StrictNestChecking && parent != null  ) parent.Unlock();
		}
	}
	/// <summary>
	/// 空要素ではない要素を表現する汎用クラスです。継承して使います。
	/// </summary>
	public class NhNotEmpty : NhBase, IDisposable
	{
		private string tagName;
		private bool startTagClosed = false;
		/// <summary>
		/// 開始タグが閉じているかどうか確認します。閉じていない場合は'&gt;'を出力して閉じます。
		/// </summary>
		protected void confirmStartTagClose()
		{
			if( !startTagClosed )
			{
				writer.Write('>');
				if( !(this is NhTextAvailable) )
				{
					writer.Write("\r\n");
				}
				startTagClosed = true;
			}
		}

		/// <summary>
		/// 一切の加工を行わず文字列を出力します。
		/// 既にXMLのフラグメント形式になったデータを出力するために使用します。
		/// 利用は推奨しません。
		/// </summary>
		/// <param name="rawString">既にXMLのフラグメント形式になった文字列</param>
		public override void WriteRawString(string rawString)
		{
			confirmStartTagClose();
			base.WriteRawString (rawString);
		}
		/// <summary>
		/// NhNotEmptyクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="tagName">タグ名。名前空間使用時には、接頭辞:ローカル名の形式</param>
		/// <param name="parent">親の要素クラス</param>
		public NhNotEmpty( TextWriter writer, string tagName, NhBase parent ) : base( writer, parent )
		{
			if( NhSetting.StrictNestChecking && parent != null ) parent.Lock();
			this.tagName = tagName;
			writer.Write('<');
			writer.Write(tagName);
		}
		/// <summary>
		/// 確保した資源を解放します。
		/// </summary>
		public void Dispose()
		{
			confirmStartTagClose();
			writer.Write("</");
			writer.Write(tagName);
			writer.Write('>');
			if( !(parent is NhTextAvailable) )
			{
				writer.Write("\r\n");
			}
			if( NhSetting.StrictNestChecking && parent != null  ) parent.Unlock();
		}
	}
	/// <summary>
	/// 内容にテキストを許す要素を表現する汎用クラスです。継承して使います。
	/// </summary>
	public class NhTextAvailable : NhNotEmpty
	{
		/// <summary>
		/// NhTextAvailableクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="tagName">タグ名。名前空間使用時には、接頭辞:ローカル名の形式</param>
		/// <param name="parent">親の要素クラス</param>
		public NhTextAvailable( TextWriter writer, string tagName, NhBase parent ) : base( writer, tagName, parent )
		{
		}
		/// <summary>
		/// テキストを出力します。XMLで特別な意味を持つ文字は、定義済み実体参照に置き換えられます。
		/// </summary>
		/// <param name="text">出力するテキスト</param>
		public void WriteText( string text )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write( NhUtil.QuoteText(text) );
		}
	}
	/// <summary>
	/// 内容がインラインとなる要素を表現する汎用クラスです。継承して使います。
	/// </summary>
	public class NhInline : NhTextAvailable
	{
		/// <summary>
		/// NhInlineクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="tagName">タグ名。名前空間使用時には、接頭辞:ローカル名の形式</param>
		/// <param name="parent">親の要素クラス</param>
		public NhInline( TextWriter writer, string tagName, NhBase parent ) : base( writer, tagName, parent )
		{
		}
		/// <summary>
		/// strong要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhStrong CreateStrong()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhStrong( writer, this );
		}
		/// <summary>
		/// em要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhEm CreateEm()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhEm( writer, this );
		}
		/// <summary>
		/// span要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhSpan CreateSpan()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhSpan( writer, this );
		}
		/// <summary>
		/// q要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhQ CreateQ()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhQ( writer, this );
		}
		/// <summary>
		/// a要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhA CreateA()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhA( writer, this );
		}
		/// <summary>
		/// img要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhImg CreateImg()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhImg( writer, this );
		}
		/// <summary>
		/// br要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhBr CreateBr()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhBr( writer, this );
		}
		/// <summary>
		/// input要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhInput CreateInput()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhInput( writer, this );
		}
		/// <summary>
		/// label要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhLabel CreateLabel()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLabel( writer, this );
		}
		/// <summary>
		/// select要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhSelect CreateSelect()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhSelect( writer, this );
		}
		/// <summary>
		/// textarea要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <param name="cols">cols属性の値</param>
		/// <param name="rows">rows属性の値</param>
		/// <returns>作成されたオブジェクト</returns>
		public NhTextArea CreateTextArea( int cols, int rows )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTextArea( writer, this, cols, rows );
		}
		/// <summary>
		/// href属性だけを持ち、内容がテキストである定型のA要素を出力します。
		/// </summary>
		/// <param name="href">href属性の値</param>
		/// <param name="text">内容となるテキスト</param>
		public void WriteAText( string href, string text )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<a href=\"");
			writer.Write(NhUtil.QuoteText(href));
			writer.Write("\">");
			writer.Write(NhUtil.QuoteText(text));
			writer.Write("</a>");
		}
		/// <summary>
		/// src, alt, width, height属性を持つimg要素を出力します。
		/// </summary>
		/// <param name="src">src属性の値</param>
		/// <param name="alt">alt属性の値</param>
		/// <param name="width">width属性の値</param>
		/// <param name="height">height属性の値</param>
		public void WriteImg( string src, string alt, int width, int height )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<img src=\"");
			writer.Write(NhUtil.QuoteText(src));
			writer.Write("\" alt=\"");
			writer.Write(NhUtil.QuoteText(alt));
			writer.Write("\" width=\"");
			writer.Write(width.ToString());
			writer.Write("\" height=\"");
			writer.Write(height.ToString());
			writer.Write("\" />");
		}
		/// <summary>
		/// src, alt属性を持つimg要素を出力します。
		/// </summary>
		/// <param name="src">src属性の値</param>
		/// <param name="alt">alt属性の値</param>
		public void WriteImg( string src, string alt )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<img src=\"");
			writer.Write(NhUtil.QuoteText(src));
			writer.Write("\" alt=\"");
			writer.Write(NhUtil.QuoteText(alt));
			writer.Write("\" />");
		}
		/// <summary>
		/// 属性を持たないbr要素を出力します。
		/// </summary>
		public void WriteBr()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<br />");
		}
	}
	/// <summary>
	/// img要素を出力するクラスです。
	/// </summary>
	public class NhImg : NhEmpty
	{
		/// <summary>
		/// NhImgクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhImg( TextWriter writer, NhBase parent ) : base( writer, "img", parent )
		{
		}
	}
	/// <summary>
	/// br要素を出力するクラスです。
	/// </summary>
	public class NhBr : NhEmpty
	{
		/// <summary>
		/// NhBrクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhBr( TextWriter writer, NhBase parent ) : base( writer, "br", parent )
		{
		}
	}
	/// <summary>
	/// span要素を出力するクラスです。
	/// </summary>
	public class NhSpan : NhInline
	{
		/// <summary>
		/// NhSpanクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhSpan( TextWriter writer, NhBase parent ) : base( writer, "span", parent )
		{
		}
	}
	/// <summary>
	/// q要素を出力するクラスです。
	/// </summary>
	public class NhQ : NhInline
	{
		/// <summary>
		/// NhQクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhQ( TextWriter writer, NhBase parent ) : base( writer, "q", parent )
		{
		}
	}
	/// <summary>
	/// strong要素を出力するクラスです。
	/// </summary>
	public class NhStrong : NhInline
	{
		/// <summary>
		/// NhStrongクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhStrong( TextWriter writer, NhBase parent ) : base( writer, "strong", parent )
		{
		}
	}
	/// <summary>
	/// em要素を出力するクラスです。
	/// </summary>
	public class NhEm : NhInline
	{
		/// <summary>
		/// NhEmクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhEm( TextWriter writer, NhBase parent ) : base( writer, "em", parent )
		{
		}
	}
	/// <summary>
	/// a要素を出力するクラスです。
	/// </summary>
	public class NhA : NhInline
	{
		/// <summary>
		/// NhAクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhA( TextWriter writer, NhBase parent ) : base( writer, "a", parent )
		{
		}
	}
	/// <summary>
	/// p要素を出力するクラスです。
	/// </summary>
	public class NhP : NhInline
	{
		/// <summary>
		/// NhPクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhP( TextWriter writer, NhBase parent ) : base( writer, "p", parent )
		{
		}
	}
	/// <summary>
	/// caption要素を出力するクラスです。
	/// </summary>
	public class NhCaption : NhInline
	{
		/// <summary>
		/// NhCaptionクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhCaption( TextWriter writer, NhBase parent ) : base( writer, "caption", parent )
		{
		}
	}
	/// <summary>
	/// 内容がインラインとなるli要素を出力するクラスです。
	/// </summary>
	[System.Obsolete("NhLiはNhLiInlineに置き換えられました。")]
	public class NhLi : NhInline
	{
		/// <summary>
		/// NhLiクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhLi( TextWriter writer, NhBase parent ) : base( writer, "li", parent )
		{
		}
	}
	/// <summary>
	/// 内容がインラインとなるli要素を出力するクラスです。
	/// </summary>
	public class NhLiInline : NhInline
	{
		/// <summary>
		/// NhLiInlineクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhLiInline( TextWriter writer, NhBase parent ) : base( writer, "li", parent )
		{
		}
	}
	/// <summary>
	/// 内容がブロックとなるli要素を出力するクラスです。
	/// </summary>
	public class NhLiBlock : NhBlock
	{
		/// <summary>
		/// NhLiBlockクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhLiBlock( TextWriter writer, NhBase parent ) : base( writer, "li", parent )
		{
		}
	}
	/// <summary>
	/// 内容がインラインとなるtd要素を出力するクラスです。
	/// </summary>
	public class NhTdInline : NhInline
	{
		/// <summary>
		/// NhTdInlineクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhTdInline( TextWriter writer, NhBase parent ) : base( writer, "td", parent )
		{
		}
	}
	/// <summary>
	/// 内容がブロックとなるtd要素を出力するクラスです。
	/// </summary>
	public class NhTdBlock : NhBlock
	{
		/// <summary>
		/// NhTdBlockクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhTdBlock( TextWriter writer, NhBase parent ) : base( writer, "td", parent )
		{
		}
	}
	/// <summary>
	/// 内容がインラインとなるth要素を出力するクラスです。
	/// </summary>
	public class NhThInline : NhInline
	{
		/// <summary>
		/// NhThInlineクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhThInline( TextWriter writer, NhBase parent ) : base( writer, "th", parent )
		{
		}
	}
	/// <summary>
	/// 内容がブロックとなるth要素を出力するクラスです。
	/// </summary>
	public class NhThBlock : NhBlock
	{
		/// <summary>
		/// NhThBlockクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhThBlock( TextWriter writer, NhBase parent ) : base( writer, "th", parent )
		{
		}
	}
	/// <summary>
	/// tr要素を出力するクラスです。
	/// </summary>
	public class NhTr : NhNotEmpty
	{
		/// <summary>
		/// NhTrクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhTr( TextWriter writer, NhBase parent ) : base( writer, "tr", parent )
		{
		}
		/// <summary>
		/// 内容がインラインとなるtd要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhTdInline CreateTdInline()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTdInline( writer, this );
		}
		/// <summary>
		/// 内容がブロックとなるtd要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhTdBlock CreateTdBlock()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTdBlock( writer, this );
		}
		/// <summary>
		/// 内容がインラインとなるth要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhThInline CreateThInline()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhThInline( writer, this );
		}
		/// <summary>
		/// 内容がブロックとなるth要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhThBlock CreateThBlock()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhThBlock( writer, this );
		}
	}
	/// <summary>
	/// ul要素を出力するクラスです。
	/// </summary>
	public class NhUl : NhNotEmpty
	{
		/// <summary>
		/// NhUlクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhUl( TextWriter writer, NhBase parent ) : base( writer, "ul", parent )
		{
		}
		/// <summary>
		/// li要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		[System.Obsolete()]
		public NhLi CreateLi()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLi( writer, this );
		}

		/// <summary>
		/// 内容がインラインとなるli要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhLiInline CreateLiInline()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLiInline( writer, this );
		}

		/// <summary>
		/// 内容がブロックとなるli要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhLiBlock CreateLiBlock()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLiBlock( writer, this );
		}

		/// <summary>
		/// 属性を持たず、内容にテキストを持つli要素を出力します。
		/// </summary>
		/// <param name="text">内容となるテキスト</param>
		public void WriteLiText( string text )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<li>");
			writer.Write(NhUtil.QuoteText(text));
			writer.Write("</li>\r\n");
		}
	}
	/// <summary>
	/// ol要素を出力するクラスです。
	/// </summary>
	public class NhOl : NhNotEmpty
	{
		/// <summary>
		/// NhOlクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhOl( TextWriter writer, NhBase parent ) : base( writer, "ol", parent )
		{
		}
		/// <summary>
		/// 内容がインラインとなるli要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		[System.Obsolete()]
		public NhLi CreateLi()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLi( writer, this );
		}

		/// <summary>
		/// 内容がインラインとなるli要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhLiInline CreateLiInline()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLiInline( writer, this );
		}

		/// <summary>
		/// 内容がブロックとなるli要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhLiBlock CreateLiBlock()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLiBlock( writer, this );
		}
		
		/// <summary>
		/// 属性を持たず、内容にテキストを持つli要素を出力します。
		/// </summary>
		/// <param name="text">内容となるテキスト</param>
		public void WriteLiText( string text )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<li>");
			writer.Write(NhUtil.QuoteText(text));
			writer.Write("</li>\r\n");
		}
	}
	/// <summary>
	/// table要素を出力するクラスです。
	/// </summary>
	public class NhTable : NhNotEmpty
	{
		/// <summary>
		/// NhTableクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhTable( TextWriter writer, NhBase parent ) : base( writer, "table", parent )
		{
		}
		/// <summary>
		/// tr要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhTr CreateTr()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTr( writer, this );
		}
		/// <summary>
		/// caption要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhCaption CreateCaption()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhCaption( writer, this );
		}
	}
	/// <summary>
	/// div要素を出力するクラスです。
	/// </summary>
	public class NhDiv : NhBlock
	{
		/// <summary>
		/// NhDivクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhDiv( TextWriter writer, NhBase parent ) : base( writer, "div", parent )
		{
		}
	}
	/// <summary>
	/// blockquote要素を出力するクラスです。
	/// </summary>
	public class NhBlockQuote : NhBlock
	{
		/// <summary>
		/// NhBlockQuoteクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhBlockQuote( TextWriter writer, NhBase parent ) : base( writer, "blockquote", parent )
		{
		}
	}
	/// <summary>
	/// h1～h6要素を出力するクラスです。
	/// </summary>
	public class NhHx : NhInline
	{
		/// <summary>
		/// NhHxクラスのコンストラクタです。
		/// </summary>
		/// <param name="level">タグ名2文字目となる1～6の整数値</param>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhHx( int level, TextWriter writer, NhBase parent ) : base( writer, "h" + level.ToString(), parent )
		{
			System.Diagnostics.Debug.Assert( level >=1 && level <= 6 );
		}
	}
	/// <summary>
	/// div要素を出力するクラスです。
	/// </summary>
	public class NhForm : NhBlock
	{
		/// <summary>
		/// NhFormクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		/// <param name="action">action属性の値</param>
		public NhForm( TextWriter writer, NhBase parent, string action ) : base( writer, "form", parent )
		{
			this.WriteAttribute("action",action);
		}
		/// <summary>
		/// 値がpostであるmethod属性を出力します。
		/// </summary>
		public void WritePostMethod()
		{
			this.WriteAttribute("method","post");
		}
		/// <summary>
		/// 値がgetであるmethod属性を出力します。
		/// </summary>
		public void WriteGetMethod()
		{
			this.WriteAttribute("method","get");
		}
	}
	/// <summary>
	/// input要素を出力するクラスです。
	/// </summary>
	public class NhInput : NhEmpty
	{
		/// <summary>
		/// NhInputクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhInput( TextWriter writer, NhBase parent ) : base( writer, "input", parent )
		{
		}
	}
	/// <summary>
	/// label要素を出力するクラスです。
	/// </summary>
	public class NhLabel : NhInline
	{
		/// <summary>
		/// NhLabelクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhLabel( TextWriter writer, NhBase parent ) : base( writer, "label", parent )
		{
		}
	}
	/// <summary>
	/// select要素を出力するクラスです。
	/// </summary>
	public class NhSelect : NhInline
	{
		/// <summary>
		/// option要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhOption CreateOption()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhOption( writer, this );
		}
		/// <summary>
		/// NhSelectクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhSelect( TextWriter writer, NhBase parent ) : base( writer, "select", parent )
		{
		}
	}
	/// <summary>
	/// option要素を出力するクラスです。
	/// </summary>
	public class NhOption : NhTextAvailable
	{
		/// <summary>
		/// NhSelectクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhOption( TextWriter writer, NhBase parent ) : base( writer, "option", parent )
		{
		}
	}
	/// <summary>
	/// textarea要素を出力するクラスです。
	/// </summary>
	public class NhTextArea : NhTextAvailable
	{
		/// <summary>
		/// NhTextAreaクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		/// <param name="cols">cols属性の値</param>
		/// <param name="rows">rows属性の値</param>
		public NhTextArea( TextWriter writer, NhBase parent, int cols, int rows ) : base( writer, "textarea", parent )
		{
			this.WriteAttribute("cols",cols.ToString());
			this.WriteAttribute("rows",rows.ToString());
		}
	}

	/// <summary>
	/// 内容がブロックとなる要素を表現する汎用クラスです。継承して使います。
	/// </summary>
	public class NhBlock : NhNotEmpty
	{
		/// <summary>
		/// NhBlockクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
 		/// <param name="tagName">タグ名。名前空間使用時には、接頭辞:ローカル名の形式</param>
		/// <param name="parent">親の要素クラス</param>
		public NhBlock( TextWriter writer, string tagName, NhBase parent ) : base( writer, tagName, parent )
		{
		}
		/// <summary>
		/// p要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhP CreateP()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhP( writer, this );
		}
		/// <summary>
		/// ul要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhUl CreateUl()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhUl( writer, this );
		}
		/// <summary>
		/// ol要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhOl CreateOl()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhOl( writer, this );
		}
		/// <summary>
		/// table要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhTable CreateTable()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTable( writer, this );
		}
		/// <summary>
		/// div要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhDiv CreateDiv()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhDiv( writer, this );
		}
		/// <summary>
		/// blockquote要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhBlockQuote CreateBlockQuote()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhBlockQuote( writer, this );
		}
		/// <summary>
		/// h1～h6要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <param name="level">タグ名2文字目となる1～6の整数値</param>
		/// <returns>作成されたオブジェクト</returns>
		public NhHx CreateHx( int level )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhHx( level, writer, this );
		}
		/// <summary>
		/// Form要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <param name="action">action属性の値</param>
		/// <returns>作成されたオブジェクト</returns>
		public NhForm CreateForm( string action )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhForm( writer, this, action );
		}
		/// <summary>
		/// 属性を持たず、内容にテキストを持つp要素を出力します。
		/// </summary>
		/// <param name="text">内容となるテキスト</param>
		public void WritePText( string text )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<p>");
			writer.Write(NhUtil.QuoteText(text));
			writer.Write("</p>\r\n");
		}
		/// <summary>
		/// 属性を持たず、内容にテキストを持つh1～h6要素を出力します。
		/// </summary>
		/// <param name="level">タグ名2文字目となる1～6の整数値</param>
		/// <param name="text">内容となるテキスト</param>
		public void WriteHxText( int level, string text )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<h");
			writer.Write(level);
			writer.Write(">");
			writer.Write(NhUtil.QuoteText(text));
			writer.Write("</h");
			writer.Write(level);
			writer.Write(">\r\n");
		}
	}
	/// <summary>
	/// body要素を出力するクラスです。
	/// </summary>
	public class NhBody : NhBlock
	{
		/// <summary>
		/// NhBodyクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhBody( TextWriter writer, NhBase parent ) : base( writer, "body", parent )
		{
		}
	}
	/// <summary>
	/// head要素を出力するクラスです。
	/// </summary>
	public class NhHead : NhNotEmpty
	{
		/// <summary>
		/// NhHeadクラスのコンストラクタです。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhHead( TextWriter writer, NhBase parent ) : base( writer, "head", parent )
		{
		}
		/// <summary>
		/// 属性を持たず、内容にテキストを持つtitle要素を出力します。
		/// </summary>
		/// <param name="title">内容となるテキスト</param>
		public void WriteTitle( string title )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<title>");
			writer.Write( NhUtil.QuoteText(title) );
			writer.Write("</title>\r\n");
		}
		/// <summary>
		/// Content-Type情報を持つmeta要素を出力します。
		/// </summary>
		/// <remarks>次のような書式が出力されます。
		/// &lt;meta http-equiv="Content-Type" content="text/html;charset="指定値" />
		/// </remarks>
		/// <param name="encodingName">charset名</param>
		public void WriteEncodingName( string encodingName )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=");
			writer.Write( encodingName );
			writer.Write( "\" />\r\n");
		}
		/// <summary>
		/// スタイルシート情報への参照を出力します。
		/// </summary>
		/// <remarks>次のような文字列が出力されます。
		/// &lt;link href="href値" type="type値" rel="stylesheet" />
		/// </remarks>
		/// <param name="href">使用するスタイルシートのURI。nullの場合、スタイルシート指定は埋め込まれません。</param>
		/// <param name="type">使用するスタイルシートのメディアタイプ (例:text/css)</param>
		public void WriteStyleSheetRefer( string href, string type )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<link href=\"");
			writer.Write(NhUtil.QuoteText(href));
			writer.Write("\" type=\"");
			writer.Write(NhUtil.QuoteText(type));
			writer.Write("\" rel=\"stylesheet\" />\r\n");
		}
	}
	/// <summary>
	/// html要素を出力するクラスです。
	/// </summary>
	public class NhHtml : NhNotEmpty
	{
		/// <summary>
		/// NhHtmlクラスのコンストラクタです。
		/// </summary>
		/// <remarks>html要素には、http://www.w3.org/1999/xhtml という値を持つxmlns属性が必ず付加されます。</remarks>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="parent">親の要素クラス</param>
		public NhHtml( TextWriter writer, NhBase parent ) : base( writer, "html", parent )
		{
			WriteAttribute( "xmlns", "http://www.w3.org/1999/xhtml" );
		}
		/// <summary>
		/// head要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhHead CreateHead()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhHead( writer, this );
		}
		/// <summary>
		/// body要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhBody CreateBody()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhBody( writer, this );
		}
	}
	/// <summary>
	/// 文書全体を表現し、文書の先頭部分を出力するクラスです。
	/// </summary>
	public class NhDocument : NhBase
	{
		/// <summary>
		/// NhDocumentクラスのコンストラクタです。XML宣言とDOCTYPE宣言が自動的に出力されます。
		/// </summary>
		/// <remarks>指定されたドキュメントの種類に従って、以下のような内容が出力されます。
		/// NhDocumentType.Xhtml11の場合、次の内容が3行に渡って出力されます。
		/// &lt;?xml version=\"1.0\"?>
		/// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
		/// "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
		/// 
		/// NhDocumentType.Xhtml10Strictの場合、次の内容が3行に渡って出力されます。
		/// &lt;?xml version=\"1.0\"?>
		/// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"
		/// "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"> 
		/// 
		/// NhDocumentType.Xhtml10Transitionalの場合、次の内容が3行に渡って出力されます。
		/// &lt;?xml version=\"1.0\"?>
		/// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
		/// "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
		/// </remarks>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="documentType">ドキュメントの種類</param>
		public NhDocument( TextWriter writer, NhDocumentType documentType ) : base( writer, null )
		{
			writer.WriteLine("<?xml version=\"1.0\"?>");
			switch( documentType )
			{
				case NhDocumentType.Xhtml11:
					writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"");
					writer.WriteLine("\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
					break;
				case NhDocumentType.Xhtml10Strict:
					writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\"");
					writer.WriteLine("\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">");
					break;
				case NhDocumentType.Xhtml10Transitional:
					writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\"");
					writer.WriteLine("\"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");
					break;
			}
		}
		/// <summary>
		/// NhDocumentクラスのコンストラクタです。XML宣言とXHTML 1.1のDOCTYPE宣言が自動的に出力されます。
		/// </summary>
		/// <remarks>次の内容が3行に渡って出力されます。
		/// &lt;?xml version=\"1.0\"?>
		/// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
		/// "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
		/// </remarks>
		/// <param name="writer">出力に使用するライター</param>
		public NhDocument( TextWriter writer ) : base( writer, null )
		{
			writer.WriteLine("<?xml version=\"1.0\"?>");
			writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"");
			writer.WriteLine("\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
		}
		/// <summary>
		/// html要素に対応するオブジェクトを生成します。
		/// </summary>
		/// <returns>作成されたオブジェクト</returns>
		public NhHtml CreateHtml()
		{
			return new NhHtml( writer, this );
		}
	}
	/// <summary>
	/// 文書全体を表現し、定型化されたヘッダーを出力するクラスです。
	/// 少ない手間で定型文書を出力する場合に使用します。
	/// </summary>
	public class NhQuickDocument : IDisposable
	{
		private NhHtml html;
		private NhBody body;
		/// <summary>
		/// NhQuickDocumentクラスのコンストラクタです。文書のヘッダーに含まれる情報を引数に指定します。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="title">文書のタイトル(title要素)</param>
		/// <param name="styleSheetHref">使用するスタイルシートのURI。nullの場合、スタイルシート指定は埋め込まれません。</param>
		/// <param name="styleSheetType">使用するスタイルシートのメディアタイプ (例:text/css)</param>
		[System.Obsolete()]
		public NhQuickDocument( TextWriter writer, string title, 
			string styleSheetHref, string styleSheetType )
		{
			NhDocument nestedDocument = new NhDocument( writer );
			html = nestedDocument.CreateHtml();
			using( NhHead head = html.CreateHead() )
			{
				head.WriteTitle( title );
				if( styleSheetHref != null )
				{
					head.WriteStyleSheetRefer( styleSheetHref, styleSheetType );
				}
			}
			body = html.CreateBody();
		}
		/// <summary>
		/// NhQuickDocumentクラスのコンストラクタです。文書のヘッダーに含まれる情報を引数に指定します。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="title">文書のタイトル(title要素)</param>
		/// <param name="styleSheetHref">使用するスタイルシートのURI。nullの場合、スタイルシート指定は埋め込まれません。</param>
		/// <param name="styleSheetType">使用するスタイルシートのメディアタイプ (例:text/css)</param>
		/// <param name="languageCode">文書のデフォルトの言語コード (例:ja-JP)</param>
		/// <param name="documentType">ドキュメントの種類</param>
		/// <remarks>ドキュメントの種類がNhDocumentType.Xhtml10Strictまたは、NhDocumentType.Xhtml10Transitionalの場合、
		/// 言語コードは、xml:lang属性の他にlang属性としても出力されます。
		/// </remarks>
		public NhQuickDocument( TextWriter writer, string title, 
			string styleSheetHref, string styleSheetType, string languageCode, NhDocumentType documentType )
		{
			NhDocument nestedDocument = new NhDocument( writer, documentType );
			html = nestedDocument.CreateHtml();
			if( documentType == NhDocumentType.Xhtml10Strict || documentType == NhDocumentType.Xhtml10Transitional )
			{
				html.WriteAttribute("lang",languageCode);
			}
			html.WriteAttribute("xml:lang",languageCode);
			using( NhHead head = html.CreateHead() )
			{
				head.WriteTitle( title );
				if( styleSheetHref != null )
				{
					head.WriteStyleSheetRefer( styleSheetHref, styleSheetType );
				}
			}
			body = html.CreateBody();
		}
		/// <summary>
		/// NhQuickDocumentクラスのコンストラクタです。文書のヘッダーに含まれる情報を引数に指定します。
		/// </summary>
		/// <param name="writer">出力に使用するライター</param>
		/// <param name="title">文書のタイトル(title要素)</param>
		/// <param name="styleSheetHref">使用するスタイルシートのURI。nullの場合、スタイルシート指定は埋め込まれません。</param>
		/// <param name="styleSheetType">使用するスタイルシートのメディアタイプ (例:text/css)</param>
		/// <param name="languageCode">文書のデフォルトの言語コード (例:ja-JP)</param>
		/// <param name="documentType">ドキュメントの種類</param>
		/// <param name="encodingName">ヘッダーに埋め込むIANA charset名</param>
		/// <remarks>ドキュメントの種類がNhDocumentType.Xhtml10Strictまたは、NhDocumentType.Xhtml10Transitionalの場合、
		/// 言語コードは、xml:lang属性の他にlang属性としても出力されます。
		/// </remarks>
		public NhQuickDocument( TextWriter writer, string title, 
			string styleSheetHref, string styleSheetType, string languageCode, NhDocumentType documentType,
			string encodingName )
		{
			NhDocument nestedDocument = new NhDocument( writer, documentType );
			html = nestedDocument.CreateHtml();
			if( documentType == NhDocumentType.Xhtml10Strict || documentType == NhDocumentType.Xhtml10Transitional )
			{
				html.WriteAttribute("lang",languageCode);
			}
			html.WriteAttribute("xml:lang",languageCode);
			using( NhHead head = html.CreateHead() )
			{
				head.WriteRawString("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=utf-8\"");
				if( documentType == NhDocumentType.Xhtml10Strict || documentType == NhDocumentType.Xhtml10Transitional )
				{
					head.WriteRawString(" /");
				}
				head.WriteRawString(">\r\n");
				head.WriteTitle( title );
				if( styleSheetHref != null )
				{
					head.WriteStyleSheetRefer( styleSheetHref, styleSheetType );
				}
			}
			body = html.CreateBody();
		}
		/// <summary>
		/// この文書のNhBodyクラスのインスタンスを参照します。通常、このプロパティを通して文書の内容を出力します。
		/// </summary>
		public NhBody B
		{
			get { return body; }
		}
		/// <summary>
		/// 確保した資源を解放します。
		/// </summary>
		public void Dispose()
		{
			body.Dispose();
			html.Dispose();
		}
	}
}
