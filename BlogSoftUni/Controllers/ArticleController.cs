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
        [Authorize]
        public ActionResult Create()
        {
            return View();
        }
        //
        //POST:Article/Create
        [HttpPost]
        [Authorize]
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
        //
        //GET: Article/Delete
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
               //Get article from database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();
                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                //Check if article exist
                if (article == null)
                {
                    return HttpNotFound();
                }
                //Pass article ti view
                return View(article);
            }
        }
        //
        //POST: Article/Delete
        [HttpPost]
        [ActionName("Delete")]
        public ActionResult DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database = new BlogDbContext())
            {
                //Get article from database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();
                //Check if article exist
                if (article == null)
                {
                    return HttpNotFound();
                }
                //Delete article from database
                database.Articles.Remove(article);
                database.SaveChanges();
                //Redirect to index page
                return RedirectToAction("Index");
            }
        }
        //
        //GET: Article/Edit
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            using (var database =new BlogDbContext())
            {
                //Get article from the database
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .First();
                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                //Check if article exist
                if (article == null)
                {
                    return HttpNotFound();
                }
                //Create the view model
                var model=new ArticleViewModel();
                model.Id = article.Id;
                model.Title = article.Title;
                model.Content = article.Content;
                //Pass the viw model to view
                return View(model);
            }
        }
        //
        //POST: Article/Edit
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            //Chek if model state is valid
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    //Get article from DB
                    var article = database.Articles
                        .FirstOrDefault(a => a.Id == model.Id);
                    //Set article properties
                    article.Title = model.Title;
                    article.Content = model.Content;
                    //Save article state in DB
                    database.Entry(article).State=EntityState.Modified;
                    database.SaveChanges();
                    //Redirect to the index page
                    return RedirectToAction("Index");
                }
            }
            //If model state is invalid,return the same view
            return View(model);
        }

        private bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);
            return isAdmin || isAuthor;
        }
    }
}
