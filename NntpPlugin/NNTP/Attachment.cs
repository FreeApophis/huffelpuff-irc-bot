using System;
using System.IO;

namespace Nntp
{
	public class Attachment
	{
		private string id;
		private string filename;
		private byte[] binaryData;
		
		public string Id
		{
			get
			{
				return id;
			}
		}
		public string Filename
		{
			get
			{
				return filename;
			}
		}
		public byte[] BinaryData
		{
			get
			{
				return binaryData;
			}
		}
		public Attachment( string id, string filename, byte[] binaryData )
		{
			this.id = id;
			this.filename = filename;
			this.binaryData = binaryData;
		}
		public void SaveAs( string path )
		{
			this.SaveAs(path, false);
		}
		public void SaveAs( string path, bool isOverwrite )
		{
			FileStream fs = null;
			if ( isOverwrite )
				fs = new FileStream(path, FileMode.Create);
			else
				fs = new FileStream(path, FileMode.CreateNew);
			fs.Write(binaryData, 0, binaryData.Length);
			fs.Close();
		}
	}
}
