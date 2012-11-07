using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Common.Logging;

namespace ui.Controllers
{
    public class ProjRefs
    {
        private XNamespace _ns = "http://schemas.microsoft.com/developer/msbuild/2003";

        protected static readonly ILog Log = LogManager.GetLogger(typeof (ProjRefs));

        public string initialDir { get; set; }


        public void Change(ProjRef[] changes)
        {
            XDocument doc = null;
            foreach (var projRef in changes)
            {
                doc = XDocument.Load(projRef.ProjectName);
                IEnumerable<XElement> result = (from refs in doc.Root.Descendants(_ns + "Reference")
                                                let element = refs.Element(_ns + "HintPath")
                                                where element != null && testExpr(element.Value, projRef.OldRef)
                                                select element);
                Trace.Assert(result.Count()==1,"Expected exactly one match "+projRef);

                var xmlRef = result.SingleOrDefault();
                Log.Debug("value before "+xmlRef.Value);
                xmlRef.Value = projRef.Ref;
                Log.Debug("value after "+xmlRef.Value);
                
               

                doc.Save(projRef.ProjectName);

            }
        }

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
                    allRefs.Add(new ProjRef{ProjectName = projFile,Ref = hintPath.Value});
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

        private bool testExpr(string value, string oldRef)
        {
            return value.ToUpperInvariant().Contains(oldRef.ToUpperInvariant());
        }

        
    }
}