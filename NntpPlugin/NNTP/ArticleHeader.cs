using System;

namespace Nntp
{
    public class ArticleHeader
    {
        private string[] referenceIds;
        private string subject;
        private DateTime date;
        private string from;
        private string sender;
        private string postingHost;
        private int lineCount;
        
        public string[] ReferenceIds
        {
            get
            {
                return referenceIds;
            }
            set
            {
                referenceIds = value;
            }
        }
        public string Subject
        {
            get
            {
                return subject;
            }
            set
            {
                subject = value;
            }
        }
        public DateTime Date
        {
            get
            {
                return date;
            }
            set
            {
                date = value;
            }
        }
        public string From
        {
            get
            {
                return from;
            }
            set
            {
                from = value;
            }
        }
        public string Sender
        {
            get
            {
                return sender;
            }
            set
            {
                sender = value;
            }
        }
        public string PostingHost
        {
            get
            {
                return postingHost;
            }
            set
            {
                postingHost = value;
            }
        }
        public int LineCount
        {
            get
            {
                return lineCount;
            }
            set
            {
                lineCount = value;
            }
        }
    }
}
