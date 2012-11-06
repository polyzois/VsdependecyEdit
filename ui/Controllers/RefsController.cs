using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Xml.Linq;

namespace ui.Controllers
{
    public class RefsController : ApiController
    {
        // GET api/values
        public ProjRef[] Get()
        {

            var parser = new ProjRefs() {initialDir = @"C:\dragon\dropbox"};

            return parser.GetAllRefs();
        }

       

        // POST api/values
        public ProjRef[] Post([FromBody] ProjRef[] updates)
        {
            return updates;
        }

        
    }


    public class ProjRef
    {
        public string Name { get; set; }
        public string Ref { get; set; }
    }

    public class ProjRefs
    {
        private XNamespace _ns = "http://schemas.microsoft.com/developer/msbuild/2003";


        public string initialDir { get; set; }

        public ProjRef[] GetAllRefs()
        {

            var projFiles = ProjFiles();

            XDocument doc = null;

            List<ProjRef> allRefs= new List<ProjRef>();

            foreach (var projFile in projFiles)
            {
                Console.WriteLine(projFile);
                doc = XDocument.Load(projFile);

                IEnumerable<XElement> result = (from refs in doc.Root.Descendants(_ns + "Reference")
                                               let element = refs.Element(_ns + "HintPath")
                                               where element != null //&& testExpr(element.Value)
                                               select element);

               

                foreach (XElement hintPath in result)
                {
                    allRefs.Add(new ProjRef{Name = projFile,Ref = hintPath.Value});
                }

            }

            return allRefs.ToArray();
        }

        private string[] ProjFiles()
        {
            var projFiles = Directory.GetFiles(initialDir,
                                               "*.csproj",
                                               SearchOption.AllDirectories);
            List<string> filtered = new List<string>();
            foreach (var projFile in projFiles)
            {
               if (!projFile.StartsWith(initialDir + "\\.git"))
               {
                   filtered.Add(projFile);
               }
            }
            return filtered.ToArray();
        }

        private bool testExpr(string value)
        {
            return value.ToUpperInvariant().Contains(RefSearchPattern.ToUpperInvariant());
        }

        public string RefSearchPattern { get; set; }
    }

}