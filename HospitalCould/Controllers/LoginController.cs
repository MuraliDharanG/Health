using HospitalCloud;
using HospitalCloud.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;

namespace HospitalCould.Controllers
{
    public class LoginController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login(string username, string password)
        {
            if (username == "admin" && password == "admin@123")
            {
                CreateAuthendicationTicket(new User() {  UserName = username, Password = password }, "Admin");
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Login");
        }

        private void CreateAuthendicationTicket(User model, string userRole)
        {

            var serializeModel = new User() { UserId = model.UserId, UserName = model.UserName, Password = model.Password, Role = userRole };
            //,UserImage = model.UserImage};
            var serializer = new JavaScriptSerializer();
            var userData = serializer.Serialize(serializeModel);
            var authTicket = new FormsAuthenticationTicket(1, model.UserName, DateTime.Now, DateTime.Now.AddHours(8), false, userData);
            var encTicket = FormsAuthentication.Encrypt(authTicket);
            var faCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encTicket);
            Response.Cookies.Add(faCookie);
        }


        public ActionResult DoctorsLogin()
        {
            return View();
        }

        [HttpPost]
        public ActionResult DoctorsLogin(string UserName, string Password, HttpPostedFileBase SecurityImage)
        {
            var selecteduser = ReadUserRegisterationTable(database()).FindAll().FirstOrDefault(s => s.Name == UserName && s.Password == Password);
            if (selecteduser != null)
            {
                string imgPath;

                SecurityImage.SaveAs("G:\\files\\"+SecurityImage.FileName);

                imgPath = "G:\\files\\"+SecurityImage.FileName;
              

                byte[] content = imgStream(imgPath);
               

                byte[] img = selecteduser.SecurityImange;
                if (content.Length == img.Length)
                {
                    CreateAuthendicationTicket(new User() { UserId = Convert.ToString(selecteduser.Id), UserName = selecteduser.UserName, Password = selecteduser.Password }, "Doctor");
                    return RedirectToAction("Index", "Home");

                }
                else
                {
                    //this.AddToastMessage("error", "username or Password is Incorrect", ToastType.Warning);
                    Response.AddHeader("Login Failed", "Login Failed");
                    System.IO.File.Delete(imgPath);

                }
            }
            else
            {
                //Label1.Text = "Incorrect User name or password!";
            }
            return View();
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

