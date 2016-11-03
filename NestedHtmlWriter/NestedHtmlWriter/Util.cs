using System;

namespace NestedHtmlWriter
{
	/// <summary>
	/// NestedHtmlWriter����Ɨp�Ɏg���@�\���܂ރN���X�ł��B
	/// </summary>
	public class NhUtil
	{
		/// <summary>
		/// ������𒲂ׁAXML�œ��ʂȈӖ������������A��`�ςݎ��̎Q�Ƃɒu�������܂��B
		/// </summary>
		/// <param name="s">�ʏ�̕�����</param>
		/// <returns>�u��������ꂽ������</returns>
		public static string QuoteText( string s )
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach( char c in s )
			{
				switch( c )
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
						// IE��&apos;���ʂ��Ȃ������̂ŃR�����g�A�E�g
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
