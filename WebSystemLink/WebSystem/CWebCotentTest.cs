using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LAB_ROOM4
{
    public class CWebCotentTest : WebContent
    {
        public void WebEventTest(string url)
        {
            base.url = url;
        }

        public override void BuildHttpQuery()
        {
            switch (mHttpType)
            {
                case eHTTPTYPE.GET:

                    break;
                case eHTTPTYPE.POST:
                    break;
                case eHTTPTYPE.REQUEST:
                    break;
                default:
                    break;
            }
        }
    }
}
