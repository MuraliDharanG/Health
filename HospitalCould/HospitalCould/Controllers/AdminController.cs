using HospitalCloud.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HospitalCloud;

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

            var ms = new MemoryStream();
            var rowno=ReadUserRegisterationTable(database()).FindAll().Count();
            SecurityImage.InputStream.CopyTo(ms);

            user.SecurityImange = ms.ToArray();
            user.DoctorId = "Doc" + rowno.ToString("D4"); 
            var saveduser = CreateUser(user, mongoDatabase);
            this.AddToastMessage("Create Doctor", "Create Doctor Sucessfull", ToastType.Info);
            return RedirectToAction("ListDoctors", "Admin");
        }

        public ActionResult EditDoctor(string Id)
        {
            var doctor = ReadUserRegisterationTable(database()).FindAll().Where(s => s.Id == ObjectId.Parse(Id)).FirstOrDefault();
            return View(doctor);
        }

        [HttpPost]
        public ActionResult EditDoctor(UserRegisteration val, string docId)
        {
            UpdateDoctorDetails(database(), docId, val);
            this.AddToastMessage("Update Doctor", "Update Doctor Information Sucessfully", ToastType.Info);
            return RedirectToAction("ListDoctors", "Admin");
        }

        private static MongoCollection<UserRegisteration> UpdateDoctorDetails(MongoDatabase mongoDatabase, string ID, UserRegisteration model)
        {
            MongoCollection<UserRegisteration> userRegisteration = mongoDatabase.GetCollection<UserRegisteration>("UserRegisteration");
            var query = Query.EQ("_id", ObjectId.Parse(ID));
            var update = Update.Set("Email", model.Email).Set("Phone", model.Phone).Set("Password", model.Password).Set("Department", model.Department).Set("Designation", model.Designation);
            userRegisteration.Update(query, update);
            return userRegisteration;
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
