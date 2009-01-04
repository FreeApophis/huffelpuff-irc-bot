using System;
using System.IO;
using System.Web;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace Nntp
{
    public delegate void OnRequestDelegate( string msg );
    
    public class NntpConnection
    {
        #region Private variables
        private TcpClient tcpClient = null;
        private StreamReader sr;
        private StreamWriter sw;

        private int timeout;
        private string connectedServer;
        private Newsgroup connectedGroup;
        private int port;
        private event OnRequestDelegate onRequest = null;
        
        private string username = null;
        private string password = null;
        #endregion

        #region Public accessors
        public int Timeout
        {
            get
            {
                return timeout;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            set
            {
                timeout = value;
                tcpClient.SendTimeout = timeout;
                tcpClient.ReceiveTimeout = timeout;
            }
        }
        public string ConnectedServer
        {
            get
            {
                return connectedServer;
            }
        }
        public Newsgroup ConnectedGroup
        {
            get
            {
                return connectedGroup;
            }
        }
        public int Port
        {
            get
            {
                return port;
            }
        }
        public event OnRequestDelegate OnRequest
        {
            [MethodImpl(MethodImplOptions.Synchronized)]
            add
            {
                onRequest += value;
            }
            [MethodImpl(MethodImplOptions.Synchronized)]
            remove
            {
                onRequest -= value;
            }
        }
        #endregion

        #region Private methods
        private void Reset()
        {
            this.connectedServer = null;
            this.connectedGroup = null;
            this.username = null;
            this.password = null;
            if ( this.tcpClient != null )
                try
                {
                    this.sw.Close();
                    this.sr.Close();
                    this.tcpClient.Close();
                }
                catch
                {
                }
            this.tcpClient = new TcpClient();
            this.tcpClient.SendTimeout = timeout;
            this.tcpClient.ReceiveTimeout = timeout;
        }
        private Response MakeRequest( string request )
        {
            if ( request != null )
            {
                sw.WriteLine(request);
                if ( onRequest != null )
                    onRequest("SEND: " + request);
            }
            string line = null;
            int code = 0;
            line = sr.ReadLine();
            if ( onRequest != null && line != null )
                onRequest("RECEIVE: " + line);
            try
            {
                code = int.Parse(line.Substring(0, 3));
            }
            catch ( NullReferenceException )
            {
                this.Reset();
                throw new NntpException(line, request);
            }
            catch ( ArgumentOutOfRangeException )
            {
                this.Reset();
                throw new NntpException(line, request);
            }
            catch ( ArgumentNullException )
            {
                this.Reset();
                throw new NntpException(line, request);
            }
            catch ( FormatException )
            {
                this.Reset();
                throw new NntpException(line, request);
            }
            if ( code == 480 )
                if ( this.SendIdentity() )
                    return MakeRequest(request);
            return new Response(code, ( line.Length >= 5 ? line.Substring(4) : null ), request);
        }
        private ArticleHeader GetHeader( string messageId, out MIMEPart part )
        {
            string response = null;
            ArticleHeader header = new ArticleHeader();
            string name = null;
            string value = null;
            header.ReferenceIds = new string[0];
            string[] values = null;
            string[] values2 = null;
            Match m = null;
            part = null;
            int i = -1;
            while ( (response = sr.ReadLine()) != null && response != "" )
            {
                m = Regex.Match(response, @"^\s+(\S+)$");
                if ( m.Success )
                {
                    value = m.Groups[1].ToString();
                }
                else
                {
                    i = response.IndexOf(':');
                    if ( i == -1 )
                    {
                        continue;
                    }
                    name = response.Substring(0, i).ToUpper();
                    value = response.Substring(i + 1);
                }
                switch ( name )
                {
                    case "REFERENCES":
                        values = value.Split(' ');
                        values2 = header.ReferenceIds;
                        header.ReferenceIds = new string[values.Length + values2.Length];
                        values.CopyTo(header.ReferenceIds, 0);
                        values2.CopyTo(header.ReferenceIds, values.Length);
                        break;
                    case "SUBJECT":
                        header.Subject += NntpUtil.Base64HeaderDecode(value);
                        break;
                    case "DATE":
                        i = value.IndexOf(',');
                        header.Date = DateTime.Parse(value.Substring(i+1, value.Length-7-i));
                        break;
                    case "FROM":
                        header.From += NntpUtil.Base64HeaderDecode(value);
                        break;
                    case "NNTP-POSTING-HOST":
                        header.PostingHost += value;
                        break;
                    case "LINES":
                        header.LineCount = int.Parse(value);
                        break;
                    case "MIME-VERSION":
                        part = new MIMEPart();
                        part.ContentType = "TEXT/PLAIN";
                        part.Charset = "US-ASCII";
                        part.ContentTransferEncoding = "7BIT";
                        part.Filename = null;
                        part.Boundary = null;
                        break;
                    case "CONTENT-TYPE":
                        if ( part != null )
                        {
                            m = Regex.Match(response, @"CONTENT-TYPE: ""?([^""\s;]+)", RegexOptions.IgnoreCase);
                            if ( m.Success )
                            {
                                part.ContentType = m.Groups[1].ToString();
                            }
                            m = Regex.Match(response, @"BOUNDARY=""?([^""\s;]+)", RegexOptions.IgnoreCase);
                            if ( m.Success )
                            {
                                part.Boundary = m.Groups[1].ToString();
                                part.EmbeddedPartList = new ArrayList();
                            }
                            m = Regex.Match(response, @"CHARSET=""?([^""\s;]+)", RegexOptions.IgnoreCase);
                            if ( m.Success )
                            {
                                part.Charset = m.Groups[1].ToString();
                            }
                            m = Regex.Match(response, @"NAME=""?([^""\s;]+)", RegexOptions.IgnoreCase);
                            if ( m.Success )
                            {
                                part.Filename = m.Groups[1].ToString();
                            }
                        }
                        break;
                    case "CONTENT-TRANSFER-ENCODING":
                        if ( part != null )
                        {
                            m = Regex.Match(response, @"CONTENT-TRANSFER-ENCODING: ""?([^""\s;]+)", RegexOptions.IgnoreCase);
                            if ( m.Success )
                            {
                                part.ContentTransferEncoding = m.Groups[1].ToString();
                            }
                        }
                        break;
                }
            }
            return header;
        }
        private ArticleBody GetNormalBody( string messageId )
        {
            char[] buff = new char[1];
            string response = null;
            ArrayList list = new ArrayList();
            StringBuilder sb = new StringBuilder();
            Attachment attach = null;
            MemoryStream ms = null;
            sr.Read(buff, 0, 1);
            int i = 0;
            byte[] bytes = null;
            Match m = null;
            while ( (response = sr.ReadLine()) != null )
            {
                if ( buff[0] == '.' )
                {
                    if ( response == "" )
                        break;
                    else
                        sb.Append(response);
                }
                else
                {
                    if ( ( buff[0] == 'B' || buff[0] == 'b' ) && ( m = Regex.Match( response, @"^EGIN \d\d\d (.+)$" , RegexOptions.IgnoreCase ) ).Success )
                    {
                        ms = new MemoryStream();
                        while ( (response = sr.ReadLine()) != null && (response.Length != 3 || response.ToUpper() != "END" ) )
                        {
                            NntpUtil.UUDecode( response, ms );
                        }
                        ms.Seek( 0, SeekOrigin.Begin );
                        bytes = new byte[ms.Length];
                        ms.Read(bytes, 0, (int)ms.Length);
                        attach = new Attachment( messageId + " - " + m.Groups[1].ToString(), m.Groups[1].ToString(), bytes );
                        list.Add(attach);
                        ms.Close();
                        i++;
                    }
                    else
                    {
                        sb.Append(buff[0]);
                        sb.Append(response);
                    }
                }
                sb.Append('\n');
                sr.Read(buff, 0, 1);
            }
            ArticleBody ab = new ArticleBody();
            ab.IsHtml = false;
            ab.Text = sb.ToString();
            ab.Attachments = (Attachment[])list.ToArray(typeof(Attachment));
            return ab;
        }
        private ArticleBody GetMIMEBody( string messageId, MIMEPart part )
        {
            string line = null;
            ArticleBody body = null;
            StringBuilder sb = null;
            ArrayList attachmentList = new ArrayList();
            try
            {
                NntpUtil.DispatchMIMEContent(sr, part, ".");
                sb = new StringBuilder();
                attachmentList = new ArrayList();
                body = new ArticleBody();
                body.IsHtml = true;
                this.ConvertMIMEContent( messageId, part, sb, attachmentList );
                body.Text = sb.ToString();
                body.Attachments = (Attachment[])attachmentList.ToArray(typeof(Attachment));
            }
            finally
            {
                if ( ((NetworkStream)sr.BaseStream).DataAvailable )
                    while ( (line = sr.ReadLine()) != null && line != "." );
            }
            return body;
        }
        private void ConvertMIMEContent( string messageId, MIMEPart part, StringBuilder sb, ArrayList attachmentList )
        {
            Match m = null;
            m = Regex.Match( part.ContentType, @"MULTIPART", RegexOptions.IgnoreCase );
            if ( m.Success )
            {
                foreach ( MIMEPart subPart in part.EmbeddedPartList )
                    this.ConvertMIMEContent(messageId, subPart, sb, attachmentList);
                return;
            }
            m = Regex.Match( part.ContentType, @"TEXT", RegexOptions.IgnoreCase );
            if ( m.Success )
            {
                sb.Append(part.Text);
                sb.Append("<hr>");
                return;
            }
            Attachment attachment = new Attachment( messageId + " - " + part.Filename, part.Filename, part.BinaryData );
            attachmentList.Add(attachment);
        }
        #endregion
        
        #region Public methods
        [MethodImpl(MethodImplOptions.Synchronized)]
        public NntpConnection()
        {
            //this.timeout = 5000;
            this.Reset();
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ConnectServer( string server, int port )
        {
            if ( this.connectedServer != null && this.connectedServer != server)
            {
                this.Disconnect();
            }
            if ( this.connectedServer != server )
            {
                tcpClient.Connect(server, port);
                NetworkStream stream = tcpClient.GetStream();
                if ( stream == null )
                    throw new NntpException("Fail to setup connection.");
                this.sr = new StreamReader(stream, Encoding.ASCII);
                this.sw = new StreamWriter(stream, Encoding.ASCII);
                this.sw.AutoFlush = true;
                this.sw.NewLine = "\r\n";
                Response res = MakeRequest(null); 
                if ( res.Code != 200 && res.Code != 201 )
                {
                    this.Reset();
                    throw new NntpException(res.Code);
                }
                this.connectedServer = server;
                this.port = port;
            }
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void ProvideIdentity( string username, string password )
        {
            if ( this.connectedServer == null )
                throw new NntpException("No connecting newsserver.");
            this.username = username;
            this.password = password;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool SendIdentity()
        {
            if ( this.username == null )
                return false;
            Response res = MakeRequest("AUTHINFO USER " + this.username);
            if ( res.Code == 381 )
            {
                res = MakeRequest("AUTHINFO PASS " + this.password);
            }
            if ( res.Code != 281 )
            {
                this.Reset();
                throw new NntpException(res.Code, "AUTHINFO PASS ******");
            }
            return true;
        }
        
        [MethodImpl(MethodImplOptions.Synchronized)]
        public Newsgroup ConnectGroup( string group )
        {
            if ( this.connectedServer == null )
                throw new NntpException("No connecting newsserver.");
            if ( this.connectedGroup == null || this.connectedGroup.Group != group )
            {
                Response res = MakeRequest("GROUP " + group); 
                if ( res.Code != 211 )
                {
                    this.connectedGroup = null;
                    throw new NntpException(res.Code, res.Request);
                }
                string[] values = res.Message.Split(' ');
                this.connectedGroup = new Newsgroup(group, int.Parse(values[1]), int.Parse(values[2]));
            }
            return this.connectedGroup;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public ArrayList GetGroupList()
        {
            if ( this.connectedServer == null )
                throw new NntpException("No connecting newsserver.");
            Response res = MakeRequest("LIST"); 
            if ( res.Code != 215 )
                throw new NntpException(res.Code, res.Request);
            ArrayList list = new ArrayList();
            string response = null;
            string[] values;
            while ( (response = sr.ReadLine()) != null && response != "." )
            {
                values = response.Split(' ');
                list.Add( new Newsgroup(values[0], int.Parse(values[2]), int.Parse(values[1])) );
            }
            return list;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public string GetMessageId( int articleId )
        {
            if ( this.connectedServer == null )
                throw new NntpException("No connecting newsserver.");
            if ( this.connectedGroup == null )
                throw new NntpException("No connecting newsgroup.");
            Response res = MakeRequest("STAT " + articleId); 
            if ( res.Code != 223 )
                throw new NntpException(res.Code, res.Request);
            int i = res.Message.IndexOf('<');
            int j = res.Message.IndexOf('>');
            return res.Message.Substring(i, j - i + 1);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public int GetArticleId( string messageId )
        {
            if ( this.connectedServer == null )
                throw new NntpException("No connecting newsserver.");
            if ( this.connectedGroup == null )
                throw new NntpException("No connecting newsgroup.");
            Response res = MakeRequest("STAT " + messageId); 
            if ( res.Code != 223 )
                throw new NntpException(res.Code, res.Request);
            int i = res.Message.IndexOf(' ');
            return int.Parse(res.Message.Substring(0, i));
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public ArrayList GetArticleList( int low, int high )
        {
            if ( this.connectedServer == null )
                throw new NntpException("No connecting newsserver.");
            if ( this.connectedGroup == null )
                throw new NntpException("No connecting newsgroup.");
            Response res = MakeRequest("XOVER " + low + "-" + high);
            if ( res.Code != 224 )
                throw new NntpException(res.Code, res.Request);
            ArrayList list = new ArrayList();
            Article article = null;
            string[] values = null;
            int i;
            string response = null;
            while ( (response = sr.ReadLine()) != null && response != "." )
            {
                try
                {
                article = new Article();
                article.Header = new ArticleHeader();
                values = response.Split('\t');
                article.ArticleId = int.Parse(values[0]);
                article.Header.Subject = NntpUtil.Base64HeaderDecode(values[1]);
                article.Header.From = NntpUtil.Base64HeaderDecode(values[2]);
                i = values[3].IndexOf(',');
                article.Header.Date = DateTime.Parse(values[3].Substring(i+1, values[3].Length-7-i));
                article.MessageId = values[4];
                if ( values[5].Trim().Length == 0 )
                    article.Header.ReferenceIds =  new string[0];
                else
                    article.Header.ReferenceIds = values[5].Split(' ');
                if ( values.Length < 8 || values[7] == null || values[7].Trim() == "" )
                    article.Header.LineCount = 0;
                else
                    article.Header.LineCount = int.Parse(values[7]);
                 
                article.Body = null;
                }
                catch ( Exception e )
                {
                    throw new Exception(response, e);
                }
                list.Add(article);
            }
            return list;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public Article GetArticle( int articleId )
        {
            return GetArticle(articleId.ToString());
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public Article GetArticle( string messageId )
        {
            if ( this.connectedServer == null )
                throw new NntpException("No connecting newsserver.");
            if ( this.connectedGroup == null )
                throw new NntpException("No connecting newsgroup.");
            Article article = new Article();
            Response res = MakeRequest("Article " + messageId);
            if ( res.Code != 220 )
                throw new NntpException(res.Code);
            int i = res.Message.IndexOf(' ');
            article.ArticleId = int.Parse(res.Message.Substring(0, i));
            article.MessageId = res.Message.Substring(i + 1, res.Message.IndexOf(' ', i+1));
            MIMEPart part = null;
            article.Header = this.GetHeader(messageId, out part);
            if ( part == null )
                article.Body = this.GetNormalBody(messageId);
            else
                article.Body = this.GetMIMEBody(messageId, part);
            return article;
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void PostArticle( Article article )
        {
            if ( this.connectedServer == null )
                throw new NntpException("No connecting newsserver.");
            if ( this.connectedGroup == null )
                throw new NntpException("No connecting newsgroup.");
            Response res = MakeRequest("POST"); 
            if ( res.Code != 340 )
                throw new NntpException(res.Code, res.Request);
            StringBuilder sb = new StringBuilder();
            sb.Append("From: ");
            sb.Append(article.Header.From);
            sb.Append("\r\nNewsgroup: ");
            sb.Append(this.connectedGroup);
            if ( article.Header.ReferenceIds != null && article.Header.ReferenceIds.Length != 0 )
            {
                sb.Append("\r\nReference: ");
                sb.Append( string.Join(" ", article.Header.ReferenceIds) );
            }
            sb.Append("\r\nSubject: ");
            sb.Append(article.Header.Subject);
            sb.Append("\r\n\r\n");
            sb.Append(article.Body.Text.Replace("\n.", "\n.."));
            sb.Append("\r\n.\r\n");
            res = MakeRequest(sb.ToString());
            if ( res.Code != 240 )
                throw new NntpException(res.Code, res.Request);
        }
        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Disconnect()
        {
            if ( this.connectedServer != null )
            {
                string response = null;
                if ( ((NetworkStream)sr.BaseStream).DataAvailable )
                {
                    while ( (response = sr.ReadLine()) != null && response != "." );
                }
                Response res = MakeRequest("QUIT");
                if ( res.Code != 205 )
                    throw new NntpException(res.Code, res.Request);
            }
            this.Reset();
        }
        #endregion
        
        private class Response
        {
            private int code;
            private string message;
            private string request;
            
            public Response( int code, string message, string request )
            {
                this.code = code;
                this.message = message;
                this.request = request;
            }
            public int Code
            {
                get
                {
                    return code;
                }
                set
                {
                    code = value;
                }
            }
            public string Message
            {
                get
                {
                    return message;
                }
                set
                {
                    message = value;
                }
            }
            public string Request
            {
                get
                {
                    return request;
                }
                set
                {
                    request = value;
                }
            }
        }

    }
}