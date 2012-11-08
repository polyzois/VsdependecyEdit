using System.Net.Http;
using System.Web.Http;
using Common.Logging;

namespace ui.Controllers
{
    public class RefsController : ApiController
    {

        protected static readonly ILog Log = LogManager.GetLogger(typeof (RefsController));

        // GET api/values
        public ProjRef[] Get([FromUri] string initialDir)
        {

            Log.Debug("directory "+initialDir);

            var parser = new ProjRefs() { initialDir = initialDir };

            return parser.GetAllRefs();
        }
       

        // POST api/values
        public ProjRef[] Post([FromBody] ProjRef[] updates, [FromUri] string initialDir)
        {
            var parser = new ProjRefs() { initialDir = initialDir };
           return  parser.Change(updates);
           
        }

        
    }
}