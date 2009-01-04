using System;

namespace Nntp
{
    public class ArticleBody
    {
        private bool isHtml;
        private string text;
        private Attachment[] attachments;
        
        public bool IsHtml
        {
            get
            {
                return isHtml;
            }
            set
            {
                isHtml = value;
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
        public Attachment[] Attachments
        {
            get
            {
                return attachments;
            }
            set
            {
                attachments = value;
            }
        }
    }
}
