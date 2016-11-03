using System;
using System.IO;

namespace NestedHtmlWriter
{
	/// <summary>
	/// �����̎�ނ������܂�
	/// </summary>
	public enum NhDocumentType
	{
		/// <summary>
		/// XHTML 1.1�������܂�
		/// </summary>
		Xhtml11,
		/// <summary>
		/// XHTML 1.0 Strict�������܂�
		/// </summary>
		Xhtml10Strict,
		/// <summary>
		/// XHTML 1.0 Transitional�������܂�
		/// </summary>
		Xhtml10Transitional
	}

	/// <summary>
	/// NestedHtmlWriter�̃O���[�o���Ȑݒ��ێ����܂��B
	/// </summary>
	public class NhSetting
	{
		/// <summary>
		/// �����ȃ`�F�b�N���s�����ǂ������w�肵�܂��B�f�t�H���g�͍s���܂��B
		/// </summary>
		/// <remarks>
		/// �����ȃ`�F�b�N���s�����ǂ������w�肵�܂��B�f�t�H���g�͍s���܂��B
		/// �����ȃ`�F�b�N���s���ƁA�����̐������Ȃ��o�͂�NhException��O�𔭐������邱�Ƃ��ł��A���S�ł��B
		/// �������A���x���d�v�ȏꍇ�ɂ�false�Ƃ��ă`�F�b�N���Ȃ����Ƃ��ł��܂��B
		/// </remarks>
		public static bool StrictNestChecking = true;
	}
	/// <summary>
	/// �s���ȑg�ݍ��킹�̏o�͂��s�����Ƃ��Ă���󋵂œ��������O�ł��B
	/// </summary>
	public class NhException : Exception
	{
		/// <summary>
		/// NhException�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		public NhException()
		{
		}
		/// <summary>
		/// NhException�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		public NhException(string msg) : base(msg)
		{
		}
		/// <summary>
		/// NhException�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		public NhException(string msg, Exception ex) : base(msg,ex)
		{
		}
	}
	/// <summary>
	/// NestedHtmlWriter�̑S�Ă̏o�̓N���X�̊��ƂȂ�N���X�ł��B
	/// </summary>
	public class NhBase
	{
		/// <summary>
		/// �o�͂Ɏg�p���郉�C�^�[�ł��B
		/// </summary>
		protected TextWriter writer;
		/// <summary>
		/// �e�̗v�f�N���X��ێ����܂��B
		/// </summary>
		protected NhBase parent;
		/// <summary>
		/// ���b�N����Ă��邩���ׂāA��O�𓊂��܂��B
		/// </summary>
		/// <remarks>
		/// �����ȃ`�F�b�N���s��Ȃ��ꍇ�͌Ăяo���Ă͂����܂���B
		/// </remarks>
		protected void checkLock()
		{
			if( lockFlag )
			{
				throw new NhException("NestedHtmlWriter�A�C�e���̓��b�N����Ă��܂��B");
			}
		}
		/// <summary>
		/// NhBase�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhBase( TextWriter writer, NhBase parent )
		{
			this.writer = writer;
			this.parent = parent;
		}
		/// <summary>
		/// ��؂̉��H���s�킸��������o�͂��܂��B
		/// ����XML�̃t���O�����g�`���ɂȂ����f�[�^���o�͂��邽�߂Ɏg�p���܂��B
		/// ���p�͐������܂���B
		/// </summary>
		/// <param name="rawString">����XML�̃t���O�����g�`���ɂȂ���������</param>
		public virtual void WriteRawString( string rawString )
		{
			writer.Write(rawString);
		}
		/// <summary>
		/// �C�ӂ̑������o�͂��܂��B
		/// </summary>
		/// <param name="name">�����̖��O�B���O��Ԏg�p���ɂ́A�ړ���:���[�J�����̌`���B</param>
		/// <param name="val">�����̒l</param>
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
		/// class�������o�͂��܂��B
		/// </summary>
		/// <param name="val">�����̒l</param>
		public void WriteClassAttr( string val )
		{
			WriteAttribute( "class", val );
		}
		/// <summary>
		/// id�������o�͂��܂��B
		/// </summary>
		/// <param name="val">�����̒l</param>
		public void WriteIDAttr( string val )
		{
			WriteAttribute( "id", val );
		}
		private bool lockFlag = false;
		internal void Lock()
		{
			if( lockFlag )
			{
				throw new NhException("���Ƀ��b�N����Ă���NestedHtmlWriter�̃A�C�e�������b�N���悤�Ƃ��܂����B");
			}
			lockFlag = true;
		}
		internal void Unlock()
		{
			if( !lockFlag )
			{
				throw new NhException("���b�N����Ă��Ȃ�NestedHtmlWriter�̃A�C�e�������b�N�������悤�Ƃ��܂����B");
			}
			lockFlag = false;
		}
	}
	/// <summary>
	/// ��v�f��\������ėp�N���X�ł��B�p�����Ďg���܂��B
	/// </summary>
	public class NhEmpty : NhBase, IDisposable
	{
		private string tagName;
		/// <summary>
		/// NhEmpty�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="tagName">�^�O���B���O��Ԏg�p���ɂ́A�ړ���:���[�J�����̌`��</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhEmpty( TextWriter writer, string tagName, NhBase parent ) : base( writer, parent )
		{
			if( NhSetting.StrictNestChecking && parent != null ) parent.Lock();
			this.tagName = tagName;
			writer.Write('<');
			writer.Write(tagName);
		}
		/// <summary>
		/// �m�ۂ���������������܂��B
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
	/// ��v�f�ł͂Ȃ��v�f��\������ėp�N���X�ł��B�p�����Ďg���܂��B
	/// </summary>
	public class NhNotEmpty : NhBase, IDisposable
	{
		private string tagName;
		private bool startTagClosed = false;
		/// <summary>
		/// �J�n�^�O�����Ă��邩�ǂ����m�F���܂��B���Ă��Ȃ��ꍇ��'&gt;'���o�͂��ĕ��܂��B
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
		/// ��؂̉��H���s�킸��������o�͂��܂��B
		/// ����XML�̃t���O�����g�`���ɂȂ����f�[�^���o�͂��邽�߂Ɏg�p���܂��B
		/// ���p�͐������܂���B
		/// </summary>
		/// <param name="rawString">����XML�̃t���O�����g�`���ɂȂ���������</param>
		public override void WriteRawString(string rawString)
		{
			confirmStartTagClose();
			base.WriteRawString (rawString);
		}
		/// <summary>
		/// NhNotEmpty�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="tagName">�^�O���B���O��Ԏg�p���ɂ́A�ړ���:���[�J�����̌`��</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhNotEmpty( TextWriter writer, string tagName, NhBase parent ) : base( writer, parent )
		{
			if( NhSetting.StrictNestChecking && parent != null ) parent.Lock();
			this.tagName = tagName;
			writer.Write('<');
			writer.Write(tagName);
		}
		/// <summary>
		/// �m�ۂ���������������܂��B
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
	/// ���e�Ƀe�L�X�g�������v�f��\������ėp�N���X�ł��B�p�����Ďg���܂��B
	/// </summary>
	public class NhTextAvailable : NhNotEmpty
	{
		/// <summary>
		/// NhTextAvailable�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="tagName">�^�O���B���O��Ԏg�p���ɂ́A�ړ���:���[�J�����̌`��</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhTextAvailable( TextWriter writer, string tagName, NhBase parent ) : base( writer, tagName, parent )
		{
		}
		/// <summary>
		/// �e�L�X�g���o�͂��܂��BXML�œ��ʂȈӖ����������́A��`�ςݎ��̎Q�Ƃɒu���������܂��B
		/// </summary>
		/// <param name="text">�o�͂���e�L�X�g</param>
		public void WriteText( string text )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write( NhUtil.QuoteText(text) );
		}
	}
	/// <summary>
	/// ���e���C�����C���ƂȂ�v�f��\������ėp�N���X�ł��B�p�����Ďg���܂��B
	/// </summary>
	public class NhInline : NhTextAvailable
	{
		/// <summary>
		/// NhInline�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="tagName">�^�O���B���O��Ԏg�p���ɂ́A�ړ���:���[�J�����̌`��</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhInline( TextWriter writer, string tagName, NhBase parent ) : base( writer, tagName, parent )
		{
		}
		/// <summary>
		/// strong�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhStrong CreateStrong()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhStrong( writer, this );
		}
		/// <summary>
		/// em�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhEm CreateEm()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhEm( writer, this );
		}
		/// <summary>
		/// span�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhSpan CreateSpan()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhSpan( writer, this );
		}
		/// <summary>
		/// q�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhQ CreateQ()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhQ( writer, this );
		}
		/// <summary>
		/// a�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhA CreateA()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhA( writer, this );
		}
		/// <summary>
		/// img�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhImg CreateImg()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhImg( writer, this );
		}
		/// <summary>
		/// br�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhBr CreateBr()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhBr( writer, this );
		}
		/// <summary>
		/// input�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhInput CreateInput()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhInput( writer, this );
		}
		/// <summary>
		/// label�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhLabel CreateLabel()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLabel( writer, this );
		}
		/// <summary>
		/// select�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhSelect CreateSelect()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhSelect( writer, this );
		}
		/// <summary>
		/// textarea�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <param name="cols">cols�����̒l</param>
		/// <param name="rows">rows�����̒l</param>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhTextArea CreateTextArea( int cols, int rows )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTextArea( writer, this, cols, rows );
		}
		/// <summary>
		/// href���������������A���e���e�L�X�g�ł����^��A�v�f���o�͂��܂��B
		/// </summary>
		/// <param name="href">href�����̒l</param>
		/// <param name="text">���e�ƂȂ�e�L�X�g</param>
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
		/// src, alt, width, height����������img�v�f���o�͂��܂��B
		/// </summary>
		/// <param name="src">src�����̒l</param>
		/// <param name="alt">alt�����̒l</param>
		/// <param name="width">width�����̒l</param>
		/// <param name="height">height�����̒l</param>
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
		/// src, alt����������img�v�f���o�͂��܂��B
		/// </summary>
		/// <param name="src">src�����̒l</param>
		/// <param name="alt">alt�����̒l</param>
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
		/// �����������Ȃ�br�v�f���o�͂��܂��B
		/// </summary>
		public void WriteBr()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<br />");
		}
	}
	/// <summary>
	/// img�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhImg : NhEmpty
	{
		/// <summary>
		/// NhImg�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhImg( TextWriter writer, NhBase parent ) : base( writer, "img", parent )
		{
		}
	}
	/// <summary>
	/// br�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhBr : NhEmpty
	{
		/// <summary>
		/// NhBr�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhBr( TextWriter writer, NhBase parent ) : base( writer, "br", parent )
		{
		}
	}
	/// <summary>
	/// span�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhSpan : NhInline
	{
		/// <summary>
		/// NhSpan�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhSpan( TextWriter writer, NhBase parent ) : base( writer, "span", parent )
		{
		}
	}
	/// <summary>
	/// q�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhQ : NhInline
	{
		/// <summary>
		/// NhQ�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhQ( TextWriter writer, NhBase parent ) : base( writer, "q", parent )
		{
		}
	}
	/// <summary>
	/// strong�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhStrong : NhInline
	{
		/// <summary>
		/// NhStrong�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhStrong( TextWriter writer, NhBase parent ) : base( writer, "strong", parent )
		{
		}
	}
	/// <summary>
	/// em�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhEm : NhInline
	{
		/// <summary>
		/// NhEm�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhEm( TextWriter writer, NhBase parent ) : base( writer, "em", parent )
		{
		}
	}
	/// <summary>
	/// a�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhA : NhInline
	{
		/// <summary>
		/// NhA�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhA( TextWriter writer, NhBase parent ) : base( writer, "a", parent )
		{
		}
	}
	/// <summary>
	/// p�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhP : NhInline
	{
		/// <summary>
		/// NhP�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhP( TextWriter writer, NhBase parent ) : base( writer, "p", parent )
		{
		}
	}
	/// <summary>
	/// caption�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhCaption : NhInline
	{
		/// <summary>
		/// NhCaption�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhCaption( TextWriter writer, NhBase parent ) : base( writer, "caption", parent )
		{
		}
	}
	/// <summary>
	/// ���e���C�����C���ƂȂ�li�v�f���o�͂���N���X�ł��B
	/// </summary>
	[System.Obsolete("NhLi��NhLiInline�ɒu���������܂����B")]
	public class NhLi : NhInline
	{
		/// <summary>
		/// NhLi�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhLi( TextWriter writer, NhBase parent ) : base( writer, "li", parent )
		{
		}
	}
	/// <summary>
	/// ���e���C�����C���ƂȂ�li�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhLiInline : NhInline
	{
		/// <summary>
		/// NhLiInline�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhLiInline( TextWriter writer, NhBase parent ) : base( writer, "li", parent )
		{
		}
	}
	/// <summary>
	/// ���e���u���b�N�ƂȂ�li�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhLiBlock : NhBlock
	{
		/// <summary>
		/// NhLiBlock�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhLiBlock( TextWriter writer, NhBase parent ) : base( writer, "li", parent )
		{
		}
	}
	/// <summary>
	/// ���e���C�����C���ƂȂ�td�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhTdInline : NhInline
	{
		/// <summary>
		/// NhTdInline�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhTdInline( TextWriter writer, NhBase parent ) : base( writer, "td", parent )
		{
		}
	}
	/// <summary>
	/// ���e���u���b�N�ƂȂ�td�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhTdBlock : NhBlock
	{
		/// <summary>
		/// NhTdBlock�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhTdBlock( TextWriter writer, NhBase parent ) : base( writer, "td", parent )
		{
		}
	}
	/// <summary>
	/// ���e���C�����C���ƂȂ�th�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhThInline : NhInline
	{
		/// <summary>
		/// NhThInline�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhThInline( TextWriter writer, NhBase parent ) : base( writer, "th", parent )
		{
		}
	}
	/// <summary>
	/// ���e���u���b�N�ƂȂ�th�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhThBlock : NhBlock
	{
		/// <summary>
		/// NhThBlock�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhThBlock( TextWriter writer, NhBase parent ) : base( writer, "th", parent )
		{
		}
	}
	/// <summary>
	/// tr�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhTr : NhNotEmpty
	{
		/// <summary>
		/// NhTr�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhTr( TextWriter writer, NhBase parent ) : base( writer, "tr", parent )
		{
		}
		/// <summary>
		/// ���e���C�����C���ƂȂ�td�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhTdInline CreateTdInline()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTdInline( writer, this );
		}
		/// <summary>
		/// ���e���u���b�N�ƂȂ�td�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhTdBlock CreateTdBlock()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTdBlock( writer, this );
		}
		/// <summary>
		/// ���e���C�����C���ƂȂ�th�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhThInline CreateThInline()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhThInline( writer, this );
		}
		/// <summary>
		/// ���e���u���b�N�ƂȂ�th�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhThBlock CreateThBlock()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhThBlock( writer, this );
		}
	}
	/// <summary>
	/// ul�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhUl : NhNotEmpty
	{
		/// <summary>
		/// NhUl�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhUl( TextWriter writer, NhBase parent ) : base( writer, "ul", parent )
		{
		}
		/// <summary>
		/// li�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		[System.Obsolete()]
		public NhLi CreateLi()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLi( writer, this );
		}

		/// <summary>
		/// ���e���C�����C���ƂȂ�li�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhLiInline CreateLiInline()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLiInline( writer, this );
		}

		/// <summary>
		/// ���e���u���b�N�ƂȂ�li�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhLiBlock CreateLiBlock()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLiBlock( writer, this );
		}

		/// <summary>
		/// �������������A���e�Ƀe�L�X�g������li�v�f���o�͂��܂��B
		/// </summary>
		/// <param name="text">���e�ƂȂ�e�L�X�g</param>
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
	/// ol�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhOl : NhNotEmpty
	{
		/// <summary>
		/// NhOl�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhOl( TextWriter writer, NhBase parent ) : base( writer, "ol", parent )
		{
		}
		/// <summary>
		/// ���e���C�����C���ƂȂ�li�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		[System.Obsolete()]
		public NhLi CreateLi()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLi( writer, this );
		}

		/// <summary>
		/// ���e���C�����C���ƂȂ�li�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhLiInline CreateLiInline()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLiInline( writer, this );
		}

		/// <summary>
		/// ���e���u���b�N�ƂȂ�li�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhLiBlock CreateLiBlock()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhLiBlock( writer, this );
		}
		
		/// <summary>
		/// �������������A���e�Ƀe�L�X�g������li�v�f���o�͂��܂��B
		/// </summary>
		/// <param name="text">���e�ƂȂ�e�L�X�g</param>
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
	/// table�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhTable : NhNotEmpty
	{
		/// <summary>
		/// NhTable�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhTable( TextWriter writer, NhBase parent ) : base( writer, "table", parent )
		{
		}
		/// <summary>
		/// tr�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhTr CreateTr()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTr( writer, this );
		}
		/// <summary>
		/// caption�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhCaption CreateCaption()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhCaption( writer, this );
		}
	}
	/// <summary>
	/// div�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhDiv : NhBlock
	{
		/// <summary>
		/// NhDiv�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhDiv( TextWriter writer, NhBase parent ) : base( writer, "div", parent )
		{
		}
	}
	/// <summary>
	/// blockquote�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhBlockQuote : NhBlock
	{
		/// <summary>
		/// NhBlockQuote�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhBlockQuote( TextWriter writer, NhBase parent ) : base( writer, "blockquote", parent )
		{
		}
	}
	/// <summary>
	/// h1�`h6�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhHx : NhInline
	{
		/// <summary>
		/// NhHx�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="level">�^�O��2�����ڂƂȂ�1�`6�̐����l</param>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhHx( int level, TextWriter writer, NhBase parent ) : base( writer, "h" + level.ToString(), parent )
		{
			System.Diagnostics.Debug.Assert( level >=1 && level <= 6 );
		}
	}
	/// <summary>
	/// div�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhForm : NhBlock
	{
		/// <summary>
		/// NhForm�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		/// <param name="action">action�����̒l</param>
		public NhForm( TextWriter writer, NhBase parent, string action ) : base( writer, "form", parent )
		{
			this.WriteAttribute("action",action);
		}
		/// <summary>
		/// �l��post�ł���method�������o�͂��܂��B
		/// </summary>
		public void WritePostMethod()
		{
			this.WriteAttribute("method","post");
		}
		/// <summary>
		/// �l��get�ł���method�������o�͂��܂��B
		/// </summary>
		public void WriteGetMethod()
		{
			this.WriteAttribute("method","get");
		}
	}
	/// <summary>
	/// input�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhInput : NhEmpty
	{
		/// <summary>
		/// NhInput�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhInput( TextWriter writer, NhBase parent ) : base( writer, "input", parent )
		{
		}
	}
	/// <summary>
	/// label�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhLabel : NhInline
	{
		/// <summary>
		/// NhLabel�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhLabel( TextWriter writer, NhBase parent ) : base( writer, "label", parent )
		{
		}
	}
	/// <summary>
	/// select�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhSelect : NhInline
	{
		/// <summary>
		/// option�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhOption CreateOption()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhOption( writer, this );
		}
		/// <summary>
		/// NhSelect�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhSelect( TextWriter writer, NhBase parent ) : base( writer, "select", parent )
		{
		}
	}
	/// <summary>
	/// option�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhOption : NhTextAvailable
	{
		/// <summary>
		/// NhSelect�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhOption( TextWriter writer, NhBase parent ) : base( writer, "option", parent )
		{
		}
	}
	/// <summary>
	/// textarea�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhTextArea : NhTextAvailable
	{
		/// <summary>
		/// NhTextArea�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		/// <param name="cols">cols�����̒l</param>
		/// <param name="rows">rows�����̒l</param>
		public NhTextArea( TextWriter writer, NhBase parent, int cols, int rows ) : base( writer, "textarea", parent )
		{
			this.WriteAttribute("cols",cols.ToString());
			this.WriteAttribute("rows",rows.ToString());
		}
	}

	/// <summary>
	/// ���e���u���b�N�ƂȂ�v�f��\������ėp�N���X�ł��B�p�����Ďg���܂��B
	/// </summary>
	public class NhBlock : NhNotEmpty
	{
		/// <summary>
		/// NhBlock�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
 		/// <param name="tagName">�^�O���B���O��Ԏg�p���ɂ́A�ړ���:���[�J�����̌`��</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhBlock( TextWriter writer, string tagName, NhBase parent ) : base( writer, tagName, parent )
		{
		}
		/// <summary>
		/// p�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhP CreateP()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhP( writer, this );
		}
		/// <summary>
		/// ul�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhUl CreateUl()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhUl( writer, this );
		}
		/// <summary>
		/// ol�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhOl CreateOl()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhOl( writer, this );
		}
		/// <summary>
		/// table�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhTable CreateTable()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhTable( writer, this );
		}
		/// <summary>
		/// div�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhDiv CreateDiv()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhDiv( writer, this );
		}
		/// <summary>
		/// blockquote�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhBlockQuote CreateBlockQuote()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhBlockQuote( writer, this );
		}
		/// <summary>
		/// h1�`h6�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <param name="level">�^�O��2�����ڂƂȂ�1�`6�̐����l</param>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhHx CreateHx( int level )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhHx( level, writer, this );
		}
		/// <summary>
		/// Form�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <param name="action">action�����̒l</param>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhForm CreateForm( string action )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhForm( writer, this, action );
		}
		/// <summary>
		/// �������������A���e�Ƀe�L�X�g������p�v�f���o�͂��܂��B
		/// </summary>
		/// <param name="text">���e�ƂȂ�e�L�X�g</param>
		public void WritePText( string text )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<p>");
			writer.Write(NhUtil.QuoteText(text));
			writer.Write("</p>\r\n");
		}
		/// <summary>
		/// �������������A���e�Ƀe�L�X�g������h1�`h6�v�f���o�͂��܂��B
		/// </summary>
		/// <param name="level">�^�O��2�����ڂƂȂ�1�`6�̐����l</param>
		/// <param name="text">���e�ƂȂ�e�L�X�g</param>
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
	/// body�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhBody : NhBlock
	{
		/// <summary>
		/// NhBody�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhBody( TextWriter writer, NhBase parent ) : base( writer, "body", parent )
		{
		}
	}
	/// <summary>
	/// head�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhHead : NhNotEmpty
	{
		/// <summary>
		/// NhHead�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhHead( TextWriter writer, NhBase parent ) : base( writer, "head", parent )
		{
		}
		/// <summary>
		/// �������������A���e�Ƀe�L�X�g������title�v�f���o�͂��܂��B
		/// </summary>
		/// <param name="title">���e�ƂȂ�e�L�X�g</param>
		public void WriteTitle( string title )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<title>");
			writer.Write( NhUtil.QuoteText(title) );
			writer.Write("</title>\r\n");
		}
		/// <summary>
		/// Content-Type��������meta�v�f���o�͂��܂��B
		/// </summary>
		/// <remarks>���̂悤�ȏ������o�͂���܂��B
		/// &lt;meta http-equiv="Content-Type" content="text/html;charset="�w��l" />
		/// </remarks>
		/// <param name="encodingName">charset��</param>
		public void WriteEncodingName( string encodingName )
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			writer.Write("<meta http-equiv=\"Content-Type\" content=\"text/html;charset=");
			writer.Write( encodingName );
			writer.Write( "\" />\r\n");
		}
		/// <summary>
		/// �X�^�C���V�[�g���ւ̎Q�Ƃ��o�͂��܂��B
		/// </summary>
		/// <remarks>���̂悤�ȕ����񂪏o�͂���܂��B
		/// &lt;link href="href�l" type="type�l" rel="stylesheet" />
		/// </remarks>
		/// <param name="href">�g�p����X�^�C���V�[�g��URI�Bnull�̏ꍇ�A�X�^�C���V�[�g�w��͖��ߍ��܂�܂���B</param>
		/// <param name="type">�g�p����X�^�C���V�[�g�̃��f�B�A�^�C�v (��:text/css)</param>
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
	/// html�v�f���o�͂���N���X�ł��B
	/// </summary>
	public class NhHtml : NhNotEmpty
	{
		/// <summary>
		/// NhHtml�N���X�̃R���X�g���N�^�ł��B
		/// </summary>
		/// <remarks>html�v�f�ɂ́Ahttp://www.w3.org/1999/xhtml �Ƃ����l������xmlns�������K���t������܂��B</remarks>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="parent">�e�̗v�f�N���X</param>
		public NhHtml( TextWriter writer, NhBase parent ) : base( writer, "html", parent )
		{
			WriteAttribute( "xmlns", "http://www.w3.org/1999/xhtml" );
		}
		/// <summary>
		/// head�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhHead CreateHead()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhHead( writer, this );
		}
		/// <summary>
		/// body�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhBody CreateBody()
		{
			if( NhSetting.StrictNestChecking ) checkLock();
			confirmStartTagClose();
			return new NhBody( writer, this );
		}
	}
	/// <summary>
	/// �����S�̂�\�����A�����̐擪�������o�͂���N���X�ł��B
	/// </summary>
	public class NhDocument : NhBase
	{
		/// <summary>
		/// NhDocument�N���X�̃R���X�g���N�^�ł��BXML�錾��DOCTYPE�錾�������I�ɏo�͂���܂��B
		/// </summary>
		/// <remarks>�w�肳�ꂽ�h�L�������g�̎�ނɏ]���āA�ȉ��̂悤�ȓ��e���o�͂���܂��B
		/// NhDocumentType.Xhtml11�̏ꍇ�A���̓��e��3�s�ɓn���ďo�͂���܂��B
		/// &lt;?xml version=\"1.0\"?>
		/// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
		/// "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
		/// 
		/// NhDocumentType.Xhtml10Strict�̏ꍇ�A���̓��e��3�s�ɓn���ďo�͂���܂��B
		/// &lt;?xml version=\"1.0\"?>
		/// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Strict//EN"
		/// "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"> 
		/// 
		/// NhDocumentType.Xhtml10Transitional�̏ꍇ�A���̓��e��3�s�ɓn���ďo�͂���܂��B
		/// &lt;?xml version=\"1.0\"?>
		/// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN"
		/// "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
		/// </remarks>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="documentType">�h�L�������g�̎��</param>
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
		/// NhDocument�N���X�̃R���X�g���N�^�ł��BXML�錾��XHTML 1.1��DOCTYPE�錾�������I�ɏo�͂���܂��B
		/// </summary>
		/// <remarks>���̓��e��3�s�ɓn���ďo�͂���܂��B
		/// &lt;?xml version=\"1.0\"?>
		/// &lt;!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.1//EN"
		/// "http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd">
		/// </remarks>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		public NhDocument( TextWriter writer ) : base( writer, null )
		{
			writer.WriteLine("<?xml version=\"1.0\"?>");
			writer.WriteLine("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\"");
			writer.WriteLine("\"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">");
		}
		/// <summary>
		/// html�v�f�ɑΉ�����I�u�W�F�N�g�𐶐����܂��B
		/// </summary>
		/// <returns>�쐬���ꂽ�I�u�W�F�N�g</returns>
		public NhHtml CreateHtml()
		{
			return new NhHtml( writer, this );
		}
	}
	/// <summary>
	/// �����S�̂�\�����A��^�����ꂽ�w�b�_�[���o�͂���N���X�ł��B
	/// ���Ȃ���ԂŒ�^�������o�͂���ꍇ�Ɏg�p���܂��B
	/// </summary>
	public class NhQuickDocument : IDisposable
	{
		private NhHtml html;
		private NhBody body;
		/// <summary>
		/// NhQuickDocument�N���X�̃R���X�g���N�^�ł��B�����̃w�b�_�[�Ɋ܂܂����������Ɏw�肵�܂��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="title">�����̃^�C�g��(title�v�f)</param>
		/// <param name="styleSheetHref">�g�p����X�^�C���V�[�g��URI�Bnull�̏ꍇ�A�X�^�C���V�[�g�w��͖��ߍ��܂�܂���B</param>
		/// <param name="styleSheetType">�g�p����X�^�C���V�[�g�̃��f�B�A�^�C�v (��:text/css)</param>
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
		/// NhQuickDocument�N���X�̃R���X�g���N�^�ł��B�����̃w�b�_�[�Ɋ܂܂����������Ɏw�肵�܂��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="title">�����̃^�C�g��(title�v�f)</param>
		/// <param name="styleSheetHref">�g�p����X�^�C���V�[�g��URI�Bnull�̏ꍇ�A�X�^�C���V�[�g�w��͖��ߍ��܂�܂���B</param>
		/// <param name="styleSheetType">�g�p����X�^�C���V�[�g�̃��f�B�A�^�C�v (��:text/css)</param>
		/// <param name="languageCode">�����̃f�t�H���g�̌���R�[�h (��:ja-JP)</param>
		/// <param name="documentType">�h�L�������g�̎��</param>
		/// <remarks>�h�L�������g�̎�ނ�NhDocumentType.Xhtml10Strict�܂��́ANhDocumentType.Xhtml10Transitional�̏ꍇ�A
		/// ����R�[�h�́Axml:lang�����̑���lang�����Ƃ��Ă��o�͂���܂��B
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
		/// NhQuickDocument�N���X�̃R���X�g���N�^�ł��B�����̃w�b�_�[�Ɋ܂܂����������Ɏw�肵�܂��B
		/// </summary>
		/// <param name="writer">�o�͂Ɏg�p���郉�C�^�[</param>
		/// <param name="title">�����̃^�C�g��(title�v�f)</param>
		/// <param name="styleSheetHref">�g�p����X�^�C���V�[�g��URI�Bnull�̏ꍇ�A�X�^�C���V�[�g�w��͖��ߍ��܂�܂���B</param>
		/// <param name="styleSheetType">�g�p����X�^�C���V�[�g�̃��f�B�A�^�C�v (��:text/css)</param>
		/// <param name="languageCode">�����̃f�t�H���g�̌���R�[�h (��:ja-JP)</param>
		/// <param name="documentType">�h�L�������g�̎��</param>
		/// <param name="encodingName">�w�b�_�[�ɖ��ߍ���IANA charset��</param>
		/// <remarks>�h�L�������g�̎�ނ�NhDocumentType.Xhtml10Strict�܂��́ANhDocumentType.Xhtml10Transitional�̏ꍇ�A
		/// ����R�[�h�́Axml:lang�����̑���lang�����Ƃ��Ă��o�͂���܂��B
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
		/// ���̕�����NhBody�N���X�̃C���X�^���X���Q�Ƃ��܂��B�ʏ�A���̃v���p�e�B��ʂ��ĕ����̓��e���o�͂��܂��B
		/// </summary>
		public NhBody B
		{
			get { return body; }
		}
		/// <summary>
		/// �m�ۂ���������������܂��B
		/// </summary>
		public void Dispose()
		{
			body.Dispose();
			html.Dispose();
		}
	}
}
