using System.Collections.Generic;
using System.IO;
using System.Web.Http;

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
               var relativePath = ".." + lib.Substring(initialDir.Length);

                libraries.Add(new Library(){Name = relativePath});
            }
            return libraries;
        }
    }
}