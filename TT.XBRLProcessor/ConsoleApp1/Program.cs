using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.Extensions.Logging.log4net;

using TT.XBRLProcessor;
using TT.XBRLRender;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
//            var xbrlModel = new XBRLModel();
//            xbrlModel.Load(@"E:\sec\aapl-20170930.xml");
            //xbrlModel.Load(@"E:\gepsio\JeffFerguson.Gepsio.Test\XBRL-CONF-2014-12-10\Common\100-schema\102-01-SpecExample.xml"); 

             var xbrlRender = new XBRLRender();
             xbrlRender.HTMLRender(@"e:\sec\aapl-20170930.xml");
        }
    }
}
