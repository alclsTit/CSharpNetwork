using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB_ROOM4
{
    public enum eHTTPTYPE
    {
        GET = 1,
        POST,
        REQUEST
    }

    public enum eWebContentType
    {
        TEST
    }

    public abstract class WebContent : IWebContent
    {
        public string url { get; protected set; }
        public eHTTPTYPE mHttpType { get; private set; }

        public eWebContentType mContentType { get; private set; }

        public void Initialize()
        {

        }

        public abstract void BuildHttpQuery();

    }
}
