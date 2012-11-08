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

        public ProjRef[] Add(ProjRef[] newRefs)
        {
            XDocument doc = null;
            List<ProjRef> allRefs = new List<ProjRef>();

            foreach (var projRef in newRefs)
            {
                Log.Debug("handling " + projRef);
                doc = XDocument.Load(projRef.ProjectName);

                var refGroup = doc.Root.Descendants(_ns + "Reference").FirstOrDefault().Parent;

                var refName = Path.GetFileNameWithoutExtension(projRef.Ref);

                refGroup.AddFirst(new XElement(_ns + "Reference",new XAttribute("Include",refName),
                                          new XElement(_ns + "HintPath",
                                                 projRef.Ref
                                              )
                                      ));

                Log.Debug(refGroup);

              //  allRefs.Add(CreateRef(projRef.ProjectName, xmlRef));

                doc.Save(projRef.ProjectName);

            }
            return allRefs.ToArray();
        }

        public ProjRef[] Change(ProjRef[] changes)
        {
            XDocument doc = null;
            List<ProjRef> allRefs = new List<ProjRef>();

            foreach (var projRef in changes)
            {
                Log.Debug("handling "+projRef);
                doc = XDocument.Load(projRef.ProjectName);
                IEnumerable<XElement> result = (from refs in doc.Root.Descendants(_ns + "Reference")
                                                let element = refs.Element(_ns + "HintPath")
                                                where element != null && testExpr(element.Value, projRef.Ref)
                                                select element);
                if(result.Count()!=1)
                {
                    throw new Exception("Expected exactly one match "+projRef) ;
                }

                var xmlRef = result.SingleOrDefault();
                Log.Debug("value before "+xmlRef.Value);
                xmlRef.Value = projRef.NewRef;
                Log.Debug("value after "+xmlRef.Value);

                allRefs.Add(CreateRef(projRef.ProjectName,xmlRef));

                doc.Save(projRef.ProjectName);

            }
            return allRefs.ToArray();
        }

        public ProjRef[] GetAllRefs()
        {

            var projFiles = ProjFiles();

           return  GetRefsInternal(projFiles);
        }

        public ProjRef[] GetRefs(ProjRef[] updates)
        {
           var projFiles= new List<string>();
            foreach (var projRef in updates)
            {
                projFiles.Add(projRef.ProjectName);
            }
            return GetRefsInternal(projFiles.ToArray());
        }

        private ProjRef[] GetRefsInternal(string[] projFiles)
        {
            XDocument doc = null;

            List<ProjRef> allRefs = new List<ProjRef>();

            foreach (var projFile in projFiles)
            {
               Log.Debug(projFile);
                doc = XDocument.Load(projFile);

                IEnumerable<XElement> result = (from refs in doc.Root.Descendants(_ns + "Reference")
                                                let element = refs.Element(_ns + "HintPath")
                                                where element != null //&& testExpr(element.Value)
                                                select element);

               

                foreach (XElement hintPath in result)
                {
                    var projRef = CreateRef(projFile, hintPath);
                    allRefs.Add(projRef);
                }
            }

            return allRefs.ToArray();
        }

        private static ProjRef CreateRef(string projFile, XElement hintPath)
        {
            var path = Path.GetDirectoryName(projFile) + "\\" + hintPath.Value;
            var fileExists = File.Exists(path);
            Log.Debug("lib exists " + path + fileExists);
            if (!fileExists)
            {
                fileExists = File.Exists(hintPath.Value);
            }
            var projRef = new ProjRef {ProjectName = projFile, Ref = hintPath.Value, FileExists = fileExists};
            return projRef;
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