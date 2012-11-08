﻿using System.Net.Http;
using System.Web.Http;

namespace ui.Controllers
{
    public class RefsController : ApiController
    {
        // GET api/values
        public ProjRef[] Get()
        {

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