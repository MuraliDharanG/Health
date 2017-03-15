using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalCloud.Models
{
    public class PatientRegistration    
    {

        public ObjectId Id { get; set; } 
        public string PatientName { get; set; }
        public string PatientId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string DoctorId { get; set;  }

    }


}