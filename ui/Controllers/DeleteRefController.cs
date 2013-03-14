using System.Web.Http;

namespace ui.Controllers
{
    public class DeleteRefController : ApiController
    {
        public string Post([FromBody] ProjRef deleteMe, [FromUri] string initialDir)
        {
            var parser = new ProjRefs() { initialDir = initialDir };
            parser.Delete(deleteMe);
            return "ok";

        }
    }
}