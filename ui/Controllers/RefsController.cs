using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using Common.Logging;

namespace ui.Controllers
{

    public class PackagesController : ApiController
    {
        public List<Library> Get([FromUri] string initialDir)
        {
            var libs = Directory.GetFiles(initialDir+"\\packages",
                                              "*.dll",
                                              SearchOption.AllDirectories);

            var libraries = new List<Library>();
            foreach (var lib in libs)
            {
                libraries.Add(new Library(){Name = lib});
            }
            return libraries;
        }
    }

    public class Library
    {
        public string Name { get; set; }
        
    }

    public class RefsController : ApiController
    {

        protected static readonly ILog Log = LogManager.GetLogger(typeof (RefsController));

        // GET api/values
        public ProjRef[] Get([FromUri] string initialDir)
        {

            Log.Debug("directory "+initialDir);

            var parser = new ProjRefs() { initialDir = @"C:\dragon\NewsSubmission\git_dev" };

            return parser.GetAllRefs();
        }

       

        // POST api/values
        public ProjRef[] Post([FromBody] ProjRef[] updates)
        {
            var parser = new ProjRefs() { initialDir = @"C:\dragon\dropbox" };
           return  parser.Change(updates);
           
        }

        
    }
}