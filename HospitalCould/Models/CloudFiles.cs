using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HospitalCloud.Models
{
    public class CloudFiles
    {
        public ObjectId Id { get; set; }
        public string DoctorId { get; set; }  
        public string PatientName { get; set; }
        public string PatientId { get; set; }
        public string DoctorName { get; set; }
        public string HospitalName { get; set; }
        public string Address { get; set; }
        public string State { get; set; }
        public string Filename { get; set; }
        public string topic { get; set; }
        public string SecretKey { get; set; }
        public string Contentype { get; set; }
        public byte[] Data { get; set; }
        public DateTime Date { get; set; }


    }
}