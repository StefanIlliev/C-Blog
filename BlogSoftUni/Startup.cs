using BlogSoftUni.Models;
using BlogSoftUni.Migrations;
using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.Data.Entity;

[assembly: OwinStartupAttribute(typeof(BlogSoftUni.Startup))]
namespace BlogSoftUni
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer(
                new MigrateDatabaseToLatestVersion<BlogDbContext, Migrations.Configuration>());
            ConfigureAuth(app);
        }
    }
}
