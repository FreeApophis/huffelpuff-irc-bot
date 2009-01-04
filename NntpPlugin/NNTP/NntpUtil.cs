using System;
using System.IO;
using System.Web;
using System.Text;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nntp
{
	public class NntpUtil
	{
		private static int[] hexValue;
		private static char[] base64PemCode = { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
			    									, 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z'
												, '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '+', '/' };
		private static byte[] base64PemConvertCode;
		
		static NntpUtil()
		{
			hexValue = new int[128];
			for ( int i = 0; i <= 9; i++ )
				hexValue[i+'0'] = i;
			for ( int i = 0; i < 6; i++ )
				hexValue[i+'A'] = i + 10;
			
			base64PemConvertCode = new byte[256];
			for ( int i = 0; i < 255; i++ )
				base64PemConvertCode[i] = (byte)255;
			for ( int i = 0; i < base64PemCode.Length; i++ )
				base64PemConvertCode[base64PemCode[i]] = (byte) i;
		}
												
		public static int UUDecode( string line, Stream outputStream )
		{
			return UUDecode( line.ToCharArray(), outputStream );
		}

		public static int UUDecode( char[] line, Stream outputStream )
		{
			if ( line.Length < 1 )
				throw new InvalidOperationException("Invalid line: " + new String(line) + ".");
			if ( line[0] == '`' )
				return 0;
			uint[] line2 = new uint[line.Length];
			for ( int ii = 0; ii < line.Length; ii++ )
				line2[ii] = (uint)line[ii] - 32 & 0x3f;
			int length = (int)line2[0];
			if ( (int)(length / 3.0 + 0.999999999) * 4 > line.Length - 1 )
				throw new InvalidOperationException("Invalid length(" + length + ") with line: " + new String(line) + ".");
			
			int i = 1;
			int j = 0;
			while ( length > j + 3 )
			{
				outputStream.WriteByte( (byte)((line2[i] << 2 & 0xfc | line2[i+1] >> 4 & 0x3) & 0xff) );
				outputStream.WriteByte( (byte)((line2[i+1] << 4 & 0xf0 | line2[i+2] >> 2 & 0xf) & 0xff) );
				outputStream.WriteByte( (byte)((line2[i+2] << 6 & 0xc0 | line2[i+3] & 0x3f) & 0xff) );
				i += 4;
				j += 3;
			}
			if ( length > j )
				outputStream.WriteByte( (byte)((line2[i] << 2 & 0xfc | line2[i+1] >> 4 & 0x3) & 0xff) );
			if ( length > j + 1 )
				outputStream.WriteByte( (byte)((line2[i+1] << 4 & 0xf0 | line2[i+2] >> 2 & 0xf) & 0xff) );
			if ( length > j + 2 )
				outputStream.WriteByte( (byte)((line2[i+2] << 6 & 0xc0 | line2[i+3] & 0x3f) & 0xff) );
			return length;
		}
		
		public static int Base64Decode( string line, Stream outputStream )
		{
			return Base64Decode(line.ToCharArray(), outputStream);
		}
		
		public static int Base64Decode( char[] line, Stream outputStream )
		{
			if ( line.Length < 2 )
				throw new InvalidOperationException("Invalid line: " + new String(line) + ".");
			uint[] line2 = new uint[line.Length];
			for ( int ii = 0; ii < line.Length && line[ii] != '='; ii++ )
				line2[ii] = (uint)base64PemConvertCode[line[ii] & 0xff];

			int length;
			for ( length = line2.Length - 1; line[length] == '=' && length >= 0; length -- );
			length++;
			int i = 0;
			int j = 0;
			while ( length - i >= 4 )
			{
				outputStream.WriteByte( (byte) (line2[i] << 2 & 0xfc | line2[i+1] >> 4 & 0x3) );
				outputStream.WriteByte( (byte) (line2[i+1] << 4 & 0xf0 | line2[i+2] >> 2 & 0xf) );
				outputStream.WriteByte( (byte) (line2[i+2] << 6 & 0xc0 | line2[i+3]  & 0x3f) );
				i += 4;
				j += 3;
			}
			switch ( length - i )
			{
				case 2:
					outputStream.WriteByte( (byte) (line2[i] << 2 & 0xfc | line2[i+1] >> 4 & 0x3) );
					return j + 1;
				case 3:
					outputStream.WriteByte( (byte) (line2[i] << 2 & 0xfc | line2[i+1] >> 4 & 0x3) );
					outputStream.WriteByte( (byte) (line2[i+1] << 4 & 0xf0 | line2[i+2] >> 2 & 0xf) );
					return j + 2;
				default:
					return j;
			}
		}
		
		public static int QuotedPrintableDecode( string line, Stream outputStream )
		{
			return QuotedPrintableDecode( line.ToCharArray(), outputStream );
		}
		
		public static int QuotedPrintableDecode( char[] line, Stream outputStream )
		{
			int length = line.Length;
			int i = 0, j = 0;
			while ( i < length )
			{
				if ( line[i] == '=' )
				{
					if ( i + 2 < length )
					{
						outputStream.WriteByte( (byte)(hexValue[(int)line[i+1]] << 4 | hexValue[(int)line[i+2]]) );
						i += 3;
					}
					else
						i++;
				}
				else
				{
					outputStream.WriteByte( (byte)(line[i]) );
					i++;
				}
				j++;
			}
			if ( line[length-1] != '=' )
				outputStream.WriteByte( (byte)('\n') );
			return j;
		}
		public static MIMEPart DispatchMIMEContent( StreamReader sr, MIMEPart part, string seperator )
		{
			string line = null;
			Match m = null;
			MemoryStream ms;
			byte[] bytes;
			switch ( part.ContentType.Substring(0, part.ContentType.IndexOf('/')).ToUpper() )
			{
				case "MULTIPART":
					MIMEPart newPart = null;
					while ( (line = sr.ReadLine()) != null && line != seperator && line != seperator + "--" )
					{
						m = Regex.Match(line, @"CONTENT-TYPE: ""?([^""\s;]+)", RegexOptions.IgnoreCase);
						if ( !m.Success )
						{
							continue;
						}
						newPart = new MIMEPart();
						newPart.ContentType = m.Groups[1].ToString();
						newPart.Charset = "US-ASCII";
						newPart.ContentTransferEncoding = "7BIT";
						while ( line != "" )
						{
							m = Regex.Match(line, @"BOUNDARY=""?([^""\s;]+)", RegexOptions.IgnoreCase);
							if ( m.Success )
							{
								newPart.Boundary = m.Groups[1].ToString();
								newPart.EmbeddedPartList = new ArrayList();
							}
							m = Regex.Match(line, @"CHARSET=""?([^""\s;]+)", RegexOptions.IgnoreCase);
							if ( m.Success )
							{
								newPart.Charset = m.Groups[1].ToString();
							}		
							m = Regex.Match(line, @"CONTENT-TRANSFER-ENCODING: ""?([^""\s;]+)", RegexOptions.IgnoreCase);
							if ( m.Success )
							{
								newPart.ContentTransferEncoding = m.Groups[1].ToString();
							}
							m = Regex.Match(line, @"NAME=""?([^""\s;]+)", RegexOptions.IgnoreCase);
							if ( m.Success )
							{
								newPart.Filename = Base64HeaderDecode(m.Groups[1].ToString());
								newPart.Filename = newPart.Filename.Substring( newPart.Filename.LastIndexOfAny(new char[]{'\\', '/'}) + 1 );
							}
							line = sr.ReadLine();
						}
						part.EmbeddedPartList.Add(DispatchMIMEContent(sr, newPart, "--" + part.Boundary));
					}
					break;
				case "TEXT":
					ms = new MemoryStream();
					bytes = null;
					long pos;
					StreamReader msr = new StreamReader(ms, Encoding.GetEncoding(part.Charset));
					StringBuilder sb = new StringBuilder();
					while ( (line = sr.ReadLine()) != null && line != seperator && line != seperator + "--" )
					{
						pos = ms.Position;
						if ( line != "" )
						{
							switch ( part.ContentTransferEncoding.ToUpper() )
							{
								case "QUOTED-PRINTABLE":
									NntpUtil.QuotedPrintableDecode(line, ms);
									break;
								case "BASE64":
									if ( line != null && line != "" )
										NntpUtil.Base64Decode(line, ms);
									break;
								case "UU":
									if ( line != null && line != "" )
										NntpUtil.UUDecode(line, ms);
									break;
								case "7BIT":
									bytes = Encoding.ASCII.GetBytes(line);
									ms.Write(bytes, 0, bytes.Length);
									ms.WriteByte((byte)'\n');
									break;
								default:
									bytes = Encoding.ASCII.GetBytes(line);
									ms.Write(bytes, 0, bytes.Length);
									ms.WriteByte((byte)'\n');
									break;
							}
						}
						ms.Position = pos;
						if ( part.ContentType.ToUpper() == "TEXT/HTML" )
						{
							sb.Append( msr.ReadToEnd() );
						}
						else
						{
							sb.Append( HttpUtility.HtmlEncode( msr.ReadToEnd() ).Replace("\n", "<br>\n") ); 
						}
					}
					part.Text = sb.ToString();
					break;
				default:
					ms = new MemoryStream();
					bytes = null;
					while ( (line = sr.ReadLine()) != null && line != seperator && line != seperator + "--" )
					{
						if ( line != "" )
						{
							switch ( part.ContentTransferEncoding.ToUpper() )
							{
								case "QUOTED-PRINTABLE":
									NntpUtil.QuotedPrintableDecode(line, ms);
									break;
								case "BASE64":
									if ( line != null && line != "" )
										NntpUtil.Base64Decode(line, ms);
									break;
								case "UU":
									if ( line != null && line != "" )
										NntpUtil.UUDecode(line, ms);
									break;
								default:
									bytes = Encoding.ASCII.GetBytes(line);
									ms.Write(bytes, 0, bytes.Length);
									break;
							}
						}
					}
					ms.Seek( 0, SeekOrigin.Begin );
					part.BinaryData = new byte[ms.Length];
					ms.Read(part.BinaryData, 0, (int)ms.Length);
					break;
			}
			
			return part;
		}
		
		public static string Base64HeaderDecode( string line )
		{
			MemoryStream ms = null;
			byte[] bytes = null;
			string oStr = null;
			string code = null;
			string content = null;
			Match m = Regex.Match( line, @"=\?([^?]+)\?[^?]+\?([^?]+)\?=" );
			while ( m.Success )
			{
				ms = new MemoryStream();
				oStr = m.Groups[0].ToString();
				code = m.Groups[1].ToString();
				content = m.Groups[2].ToString();
				NntpUtil.Base64Decode(content, ms);
				ms.Seek(0, SeekOrigin.Begin);
				bytes = new byte[ms.Length];
				ms.Read(bytes, 0, bytes.Length);
				line = line.Replace(oStr, Encoding.GetEncoding(code).GetString(bytes));
				m = m.NextMatch();
			}
			return line;
		}
		
		public static ArrayList ConvertListToTree( ArrayList list )
		{
			Hashtable hash = new Hashtable(list.Count);
			ArrayList treeList = new ArrayList();
			int len;
			bool isTop;
			foreach ( Article article in list )
			{
				isTop = true;
				hash[article.MessageId] = article;
				article.LastReply = article.Header.Date;
				article.Children = new ArrayList();
				len = article.Header.ReferenceIds.Length; 
				for ( int i = 0; i < len; i++ )
				{
					if ( hash.ContainsKey(article.Header.ReferenceIds[i]) )
					{
						((Article)hash[article.Header.ReferenceIds[i]]).LastReply = article.LastReply;
						break;
					}
				}
				for ( int i = len - 1; i >= 0; i-- )
				{
					if ( hash.ContainsKey(article.Header.ReferenceIds[i]) )
					{
						isTop = false;
						((Article)hash[article.Header.ReferenceIds[i]]).Children.Add(article);
						break;
					}
				}
				if ( isTop )
					treeList.Add(article);
			}
			return treeList;
		}
	}
}