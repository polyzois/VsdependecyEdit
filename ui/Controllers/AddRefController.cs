using System.Web.Http;
using Common.Logging;

namespace ui.Controllers
{
    public class AddRefController : ApiController
    {

        protected static readonly ILog Log = LogManager.GetLogger(typeof (AddRefController));

        public ProjRef[] Get([FromUri] string initialDir)
        {
            return new ProjRef[]{};
        }
       

        public ProjRef[] Post([FromBody] ProjRef[] newRefs, [FromUri] string initialDir)
        {
            var parser = new ProjRefs() { initialDir = initialDir };
            return  parser.Add(newRefs);
           
        }

        
    }
}