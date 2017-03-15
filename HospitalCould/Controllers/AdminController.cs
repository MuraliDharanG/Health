using HospitalCloud.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HospitalCould.Controllers
{
    public class AdminController : Controller
    {

        public ActionResult ListDoctors()
        {
            var allUsers = ReadUserRegisterationTable(database());
            var userList = allUsers.FindAll().Select(s => s);
            return View(userList);
        }


        public ActionResult CreateDoctors()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }



        [HttpPost]
        public ActionResult CreateDoctors(UserRegisteration user, HttpPostedFileBase SecurityImage)
        {

            MongoDatabase mongoDatabase = database();
            string imgPath;

            SecurityImage.SaveAs("G:\\files\\" + SecurityImage.FileName);

            imgPath = "G:\\files\\" + SecurityImage.FileName;

            if (System.IO.File.Exists(imgPath))
            {
                byte[] content = imgStream(imgPath);
                user.SecurityImange = content;
                var saveduser = CreateUser(user, mongoDatabase);

            }
            return RedirectToAction("ListDoctors", "Admin");
        }

        private MongoDatabase database()
        {
            string connectionString = "Server=localhost:27017";
            Console.WriteLine("Connecting MongoDB");
            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer();
            MongoDatabase mongoDatabase = server.GetDatabase("health");
            return mongoDatabase;
        }

        [HttpGet]
        public ActionResult ListAllUser()
        {
            var allUsers = ReadUserRegisterationTable(database());
            var userList = allUsers.FindAll().Select(s => s);
            return View(userList);
        }

        private MongoCollection<UserRegisteration> CreateUser(UserRegisteration user, MongoDatabase mongoDatabase)
        {

            MongoCollection<UserRegisteration> userregister = mongoDatabase.GetCollection<UserRegisteration>("UserRegisteration");
            userregister.Insert(user);
            return userregister;
        }

        private MongoCollection<UserRegisteration> ReadUserRegisterationTable(MongoDatabase mongoDatabase)
        {
            MongoCollection<UserRegisteration> UserRegisteration = mongoDatabase.GetCollection<UserRegisteration>("UserRegisteration");
            return UserRegisteration;
        }

        private byte[] imgStream(string fileName)
        {
            MemoryStream stream = new MemoryStream();
        tryagain:
            try
            {
                Bitmap bmp = new Bitmap(fileName);
                bmp.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {
                goto tryagain;
            }

            return stream.ToArray();
        }
    }
    

}
