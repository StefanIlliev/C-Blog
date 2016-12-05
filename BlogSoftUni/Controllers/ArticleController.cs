using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlogSoftUni.Models;
using System.Net;

namespace BlogSoftUni.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        //
        // GET:Article/List
        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                //Get articles from database
                var articles = database.Articles
                    .Include(a => a.Author)
                    .ToList();
                return View(articles);

            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                //Get the article from database
                var article=database.Articles
                .Where(a => a.Id == id)
                   .Include(a => a.Author)
                   .First();

                if (article == null)
                {
                    return HttpNotFound();
                }

                return View(article);
            }
        }

        public ActionResult Create()
        {
            return View();
        }
        //
        //POST:Article/Create
        [HttpPost]
        public ActionResult Create(Article article)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    //Get author id
                    var authorId = database.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First().Id;
                    //Set articles author
                    article.AuthorId = authorId;
                    //Save article in DB
                    database.Articles.Add(article);
                    database.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(article);
        }
    }
}