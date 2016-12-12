using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BlogSoftUni.Models;
using System.Net;
using System.Reflection;

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
                    .Include(a=>a.Tags)
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
                   .Include(a=>a.Tags)
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
            using (var database=new BlogDbContext()) 
            {
                var model=new ArticleViewModel();
                model.Categories = database.Categories.OrderBy(c => c.Name)
                    .ToList();
                return View(model);
            }        
        }
        //
        //POST:Article/Create
        [HttpPost]
        [Authorize]
        public ActionResult Create(ArticleViewModel model)
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
                    var article=new Article(authorId,model.Title,model.Content,model.CategoryId);
                    this.SetArticleTags(article, model, database);
                    //Save article in DB
                    database.Articles.Add(article);
                    database.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            return View(model);
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
                    .Include(a=>a.Category)
                    .First();
                if (!IsUserAuthorizedToEdit(article))
                {
                    return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
                }
                ViewBag.TagsString = string.Join(", ", article.Tags.Select(t => t.Name));
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
                model.CategoryId = article.CategoryId;
                model.Categories = database.Categories
                    .OrderBy(c => c.Name)
                    .ToList();
                model.Tags = string.Join(", ", article.Tags.Select(t => t.Name));
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
                    article.CategoryId = model.CategoryId;
                    this.SetArticleTags(article, model, database);
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

        private void SetArticleTags(Article article, ArticleViewModel model, BlogDbContext db)
        {
            //Split tags
            var tagsSplitter = model.Tags
                .Split(new char[] {',', ' '}, StringSplitOptions.RemoveEmptyEntries);
            //Clear current article tags
            article.Tags.Clear();
            //Set new article tags
            foreach (var tagString in tagsSplitter)
            {
                //Get tag from db by its name
                Article.Tag tag = db.Tags.FirstOrDefault(t => t.Name.Equals(tagString));
                //if the tag is null,create new tag
                if (tag == null)
                {
                    tag=new Article.Tag() {Name=tagString};
                    db.Tags.Add(tag);
                }
                //Add tag to article tags
                article.Tags.Add(tag);
            }
        }

        private bool IsUserAuthorizedToEdit(Article article)
        {
            bool isAdmin = this.User.IsInRole("Admin");
            bool isAuthor = article.IsAuthor(this.User.Identity.Name);
            return isAdmin || isAuthor;
        }
    }
}
