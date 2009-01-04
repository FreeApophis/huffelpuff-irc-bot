using System;

namespace Nntp
{
    public class NntpException: Exception
    {
        private int errorCode;
        private string request;
        private string message;
        public int ErrorCode
        {
            get
            {
                return this.errorCode;
            }
        }
        public string Request
        {
            get
            {
                return this.request;
            }
        }
        public override string Message
        {
            get
            {
                return this.message;
            }
        }
        private void BuildNntpException( int errorCode, string request )
        {
            this.errorCode = errorCode;
            this.request = request;
            switch ( errorCode )
            {
                case 281:
                    this.message = "Authentication accepted.";
                    break;
                case 288:
                    this.message = "Binary data to follow.";
                    break;
                case 381:
                    this.message = "More authentication information required.";
                    break;
                case 400:
                    this.message = "Service disconnected.";
                    break;
                case 411:
                    this.message = "No such newsgroup.";
                    break;
                case 412:
                    this.message = "No newsgroup current selected.";
                    break;
                case 420:
                    this.message = "No current article has been selected.";
                    break;
                case 423:
                    this.message = "No such article number in this group.";
                    break;
                case 430:
                    this.message = "No such article found.";
                    break;
                case 436:
                    this.message = "Transfer failed - try again later.";
                    break;
                case 440:
                    this.message = "Posting not allowed.";
                    break;
                case 441:
                    this.message = "Posting failed.";
                    break;
                case 480:
                    this.message = "Authentication required.";
                    break;
                case 481:
                    this.message = "More authentication information required.";
                    break;
                case 482:
                    this.message = "Authentication rejected.";
                    break;
                case 500:
                    this.message = "Command not understood.";
                    break;
                case 501:
                    this.message = "Command syntax error.";
                    break;
                case 502:
                    this.message = "No permission.";
                    break;
                case 503:
                    this.message = "Program error, function not performed.";
                    break;
                default:
                    this.message = "Unknown error.";
                    break;
            }
        }
        public NntpException( String message ): base(message)
        {
            this.message = message;
            this.errorCode = 999;
            this.request = null;
        }
        public NntpException( int errorCode ): base()
        {
            this.BuildNntpException( errorCode, null );
        }
        public NntpException( int errorCode, string request ): base()
        {
            this.BuildNntpException( errorCode, request );
        }
        public NntpException( string response, string request ): base()
        {
            this.message = response;
            this.errorCode = 999;
            this.request = request;
        }
        public override string ToString()
        {
            if ( this.InnerException != null )
                return "Nntp:NntpException: [Request: " + this.request + "][Response: " + this.errorCode.ToString() + " " + this.message + "]\n" + this.InnerException.ToString() + "\n" + this.StackTrace;
            else
                return "Nntp:NntpException: [Request: " + this.request + "][Response: " + this.errorCode.ToString() + " " + this.message + "]\n" + this.StackTrace;
        }
    }
}