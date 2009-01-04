using System;
using System.Collections;

namespace Nntp
{
	public class MIMEPart
	{
		private byte[] binaryData;
		private string boundary;
		private string contentType;
		private string contentTransferEncoding;
		private string charset;
		private string filename;
		private string text;
		private ArrayList embeddedPartList;
		
		public byte[] BinaryData
		{
			get
			{
				return binaryData;
			}
			set
			{
				binaryData = value;
			}
		}
		public string Boundary
		{
			get
			{
				return boundary;
			}
			set
			{
				boundary = value;
			}
		}
		public string ContentType
		{
			get
			{
				return contentType;
			}
			set
			{
				contentType = value;
			}
		}
		public string ContentTransferEncoding
		{
			get
			{
				return contentTransferEncoding;
			}
			set
			{
				contentTransferEncoding = value;
			}
		}
		public string Charset
		{
			get
			{
				return charset;
			}
			set
			{
				charset = value;
			}
		}
		public string Filename
		{
			get
			{
				return filename;
			}
			set
			{
				filename = value;
			}
		}
		public string Text
		{
			get
			{
				return text;
			}
			set
			{
				text = value;
			}
		}
		public ArrayList EmbeddedPartList
		{
			get
			{
				return embeddedPartList;
			}
			set
			{
				embeddedPartList = value;
			}
		}
	}
}