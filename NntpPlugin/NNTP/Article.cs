using System;
using System.Collections;

namespace Nntp
{
	public class Article
	{
		private string messageId;
		private int articleId;
		private ArticleHeader header;
		private ArticleBody body;
		private DateTime lastReply;
		private ArrayList children;

		public string MessageId
		{
			get
			{
				return messageId;
			}
			set
			{
				messageId = value;
			}
		}
		public int ArticleId
		{
			get
			{
				return articleId;
			}
			set
			{
				articleId = value;
			}
		}
		public ArticleHeader Header
		{
			get
			{
				return header;
			}
			set
			{
				header = value;
			}
		}
		public ArticleBody Body
		{
			get
			{
				return body;
			}
			set
			{
				body = value;
			}
		}
		public DateTime LastReply
		{
			get
            {
                return lastReply;
            }
            set
            {
                lastReply = value;
            }
        }
        public ArrayList Children
        {
            get
            {
                return children;
            }
            set
            {
                children = value;
            }
        }
    }
}
