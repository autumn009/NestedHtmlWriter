using System;

namespace NestedHtmlWriter
{
    /// <summary>
    /// NestedHtmlWriterが作業用に使う機能を含むクラスです。
    /// </summary>
    public class NhUtil
    {
        /// <summary>
        /// 文字列を調べ、XMLで特別な意味を持つ文字を、定義済み実体参照に置き換えます。
        /// </summary>
        /// <param name="s">通常の文字列</param>
        /// <returns>置き換えられた文字列</returns>
        public static string QuoteText(string s)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (char c in s)
            {
                switch (c)
                {
                    case '&':
                        sb.Append("&amp;");
                        break;
                    case '<':
                        sb.Append("&lt;");
                        break;
                    case '>':
                        sb.Append("&gt;");
                        break;
                    case '"':
                        sb.Append("&quot;");
                        break;
                    // IEに&apos;が通じなかったのでコメントアウト
                    //case '\'':
                    //	sb.Append("&apos;");
                    //	break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
    }
}
