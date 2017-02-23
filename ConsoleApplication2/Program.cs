using LoveSeat;
using MyCouch;
using MyCouch.Net;
using MyCouch.Requests;
using MyCouch.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication2
{
    class Program
    {
        static void Main(string[] args)
        {
            // LoveSeat
            //// assumes localhost:5984 with no credentials if constructor is left blank
            //var client = new CouchClient("root", "111111");
            //var db = client.GetDatabase("mydb");

            //// set default design doc (not required)
            //db.SetDefaultDesignDoc("docs");

            //// get document by ID
            //Document myDoc = db.GetDocument("68e56facc00392d9a35dde46860019ad");


            // MyCouch
            string username = "root";
            string password = "111111";
            string database = "t1";
            string id = "whatever";

            string filepath = @"C:\Users\xlu.APPBRIDGE\Desktop\db_comp.pdf";
            string attachmentName = "test1";
            string documentId = "whatever";

            //var byteArr = TurnFile2Byte(filepath);
            //PostAttachment(byteArr, username, password, database, id);

            var byteArr = GetAttachment(documentId, attachmentName, username, password, database);
            var byteArrRes = byteArr.Result;

            SaveArr2File(byteArrRes);

            Console.ReadKey();

        }

        public static async Task<byte[]> GetAttachment(string documentId, string attachmentName, string username, string password, string database)
        {
            const string ipaddress = "localhost";
            const string port = "5984";

            string connString = string.Format("http://{0}:{1}@{2}:{3}", username, password, ipaddress, port);

            var client = new MyCouchClient(connString, database);

            AttachmentResponse response = await client.Attachments.GetAsync(documentId, attachmentName);

            return response.Content;
        }

        public static async void PostAttachment(byte[] byteArr, string username, string password, string database, string id)
        {
            const string ipaddress = "localhost";
            const string port = "5984";

            string connString = string.Format("http://{0}:{1}@{2}:{3}", username, password, ipaddress, port);

            var request = new PutAttachmentRequest(id, "test1", HttpContentTypes.Text, byteArr);

            var client = new MyCouchClient(connString, database);

            var response = await client.Attachments.PutAsync(request);

            Console.WriteLine(response.IsSuccess);

            Console.WriteLine(response.Reason);
        }

        public static async Task<bool> Post2CouchDB(string username, string password, string database, string id)
        {
            const string ipaddress = "localhost";
            const string port = "5984";

            string connString = string.Format("http://{0}:{1}@{3}:{4}", username, password, ipaddress, port);
            //Console.WriteLine(connString);

            using (var client = new MyCouchClient(connString, database))
            {
                Book b = new Book();
                b.Name = "This show it is updated";
                b.Author = "me";
                b.Page = 1;
                b.Type = "Fantasy Novel";

                Student s = new Student();
                s.Name = "for the first time ever to get this work";
                s.Gpa = 80;
                s.Description = "this is gonna be anwesome description that i post to the database  fucking awesome";
                s.Other = "wow this whole newtonsoft thign is actually prtty alwesome";
                s.HerBook = b;

                string jsonString = JsonConvert.SerializeObject(s);

                DocumentHeaderResponse response = await client.Documents.PutAsync(id, jsonString);

                if (!response.IsSuccess)
                {
                    return false;
                }

                return true;
            }
        }


        public static void SaveArr2File(byte[] byteArr)
        {
            if (byteArr == null)
            {
                return;
            }

            File.WriteAllBytes(@"C:\Users\xlu.APPBRIDGE\Desktop\shit.pdf", byteArr);
        }

        public static byte[] TurnFile2Byte(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                return null;
            }

            return File.ReadAllBytes(filepath);
        }


    }





    public class Student
    {
        public string Name { get; set; }
        public double Gpa { get; set; }
        public string Description { get; set; }
        public string Other { get; set; }
        public Book HerBook { get; set; }
    }

    public class Book
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Type { get; set; }
        public int Page { get; set; }
    }

}
