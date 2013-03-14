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
            List<ProjRef> addedRefs = new List<ProjRef>();

            foreach (var projRef in newRefs)
            {
                if (!CheckExistance(projRef.ProjectName, projRef.Ref))
                {
                    Log.Debug("Path not found skipping " + projRef);
                    continue;
                }
                Log.Debug("handling " + projRef);
                doc = XDocument.Load(projRef.ProjectName);


                var refName = Path.GetFileNameWithoutExtension(projRef.Ref);

               if (FindExistingReference(doc, refName).Any())
               {
                   Log.Debug("ref (name) already exists "+refName +" in "+projRef.ProjectName);
                   continue;
               }
               

               var refGroup = doc.Root.Descendants(_ns + "Reference").FirstOrDefault().Parent;

                var hintPath = new XElement(_ns + "HintPath", projRef.Ref);
                refGroup.AddFirst(new XElement(_ns + "Reference",new XAttribute("Include",refName),
                                          hintPath
                                      ));

                Log.Debug(refGroup);

                addedRefs.Add(CreateResult(projRef.ProjectName, hintPath.Value));

                doc.Save(projRef.ProjectName);

            }
            return addedRefs.ToArray();
        }

        public ProjRef[] Change(ProjRef[] changes)
        {
            XDocument doc = null;
            List<ProjRef> allRefs = new List<ProjRef>();

            foreach (var projRef in changes)
            {
                if (!CheckExistance(projRef.ProjectName, projRef.NewRef))
                {
                    Log.Debug("Path not found skipping " + projRef);
                    allRefs.Add(projRef);
                    continue;
                }
                Log.Debug("handling "+projRef);
                doc = XDocument.Load(projRef.ProjectName);
                var referenceName = Path.GetFileNameWithoutExtension(projRef.Ref);
                IEnumerable<XElement> result =  FindExistingReference(doc, referenceName);
                if(result.Count()!=1)
                {
                    Log.Info("Found " + result.Count() + " result, which was unexepcted");
                    throw new Exception("Expected exactly one match "+projRef) ;
                }

                var xmlRef = result.SingleOrDefault();
                Log.Debug("value before "+xmlRef.Value);
                xmlRef.Value = projRef.NewRef;
                Log.Debug("value after "+xmlRef.Value);

                allRefs.Add(CreateResult(projRef.ProjectName, xmlRef.Value));

                doc.Save(projRef.ProjectName);

            }
            return allRefs.ToArray();
        }

        private IEnumerable<XElement> FindExistingReference(XDocument doc, string referenceName)
        {
            return (from refs in doc.Root.Descendants(_ns + "Reference")
                    let element = refs.Element(_ns + "HintPath")
                    where element != null && StartsWith(refs.Attribute("Include").Value, referenceName)
                    select element);
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
                                                where element != null
                                                select element);

               

                foreach (XElement hintPath in result)
                {
                    var projRef = CreateResult(projFile, hintPath.Value);
                    allRefs.Add(projRef);
                }
            }

            return allRefs.ToArray();
        }

        private static ProjRef CreateResult(string projFile, string referenceHintPath)
        {
            var fileExists = CheckExistance(projFile, referenceHintPath);
            var projRef = new ProjRef {ProjectName = projFile, Ref = referenceHintPath, FileExists = fileExists};
            return projRef;
        }

        private static bool CheckExistance(string projFile, string referenceHintPath)
        {
            var path = Path.GetDirectoryName(projFile) + "\\" + referenceHintPath;
            var fileExists = File.Exists(path);
            Log.Debug("lib exists " + path + fileExists);
            if (!fileExists)
            {
                fileExists = File.Exists(referenceHintPath);
            }
            return fileExists;
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

        private bool EqualsCaseInsensitive(string value, string oldRef)
        {
            return value.ToUpperInvariant().Equals(oldRef.ToUpperInvariant());
        }

        private bool StartsWith(string value, string oldRef)
        {
            return value.ToUpperInvariant().StartsWith(oldRef.ToUpperInvariant());
        }
       
    }
}