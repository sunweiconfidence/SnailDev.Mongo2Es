using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Mongo2Es.Pages.SyncNode.Elastic
{
    public class EditModel : PageModel
    {
        private readonly Mongo2Es.Mongo.MongoClient _db;
        private readonly string database = "Mongo2Es";
        private readonly string collection = "EsNode";

        public EditModel(Mongo2Es.Mongo.MongoClient db)
        {
            _db = db;
        }

        [BindProperty]
        public Mongo2Es.ElasticSearch.EsNode Node { get; set; }

        public IActionResult OnGet(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                Node = new ElasticSearch.EsNode();
            }
            else
            {
                Node = _db.GetCollectionData<ElasticSearch.EsNode>(database, collection, $"{{'_id':new ObjectId('{id}')}}").FirstOrDefault();
                if (Node == null)
                {
                    return RedirectToPage("/SyncNode/Elastic/Index");
                }
            }

            return Page();
        }


        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Node.ID))
                _db.InsertCollectionData<ElasticSearch.EsNode>(database, collection, Node);
            else
            {
                Node.UpdateTime = DateTime.Now;
                _db.UpdateCollectionData<ElasticSearch.EsNode>(database, collection, Node);
            }

            return RedirectToPage("/SyncNode/Elastic/Index");
        }
    }
}