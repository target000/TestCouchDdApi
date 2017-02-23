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

namespace AppBridgeMyCouchTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string username = "root";
            string password = "111111";
            string database = "t1";
            string id = "whatever";

            string filepath = @"C:\Users\xlu.APPBRIDGE\Desktop\db_comp.pdf";
            string attachmentName = "test1";
            string documentId = "whatever";
            string fileOutPath = @"C:\Users\xlu.APPBRIDGE\Desktop\nice_crap.pdf";

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

            //var byteArr = TurnFile2Byte(filepath);
            //PostAttachment(byteArr, username, password, database, id);

            var byteArr = GetAttachmentFromCouch(documentId, attachmentName, username, password, database);
            var byteArrRes = byteArr.Result;

            ByteArr2File(byteArrRes, fileOutPath);

            //Console.ReadKey();
        }

        private static string SetupConnString(string username, string password, string ipAddress = "localhost", int port = 5984)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            string connString = string.Format("http://{0}:{1}@{2}:{3}", username, password, ipAddress, port);

            return connString;
        }

        public static async Task<byte[]> GetAttachmentFromCouch(string documentId, string attachmentName, string username, string password, string database)
        {
            string connString = SetupConnString(username, password);

            var client = new MyCouchClient(connString, database);
            var response = await client.Attachments.GetAsync(documentId, attachmentName);

            return response.Content;
        }

        public static async void PostAttachment2Couch(byte[] byteArr, string username, string password, string database, string id)
        {
            string connString = SetupConnString(username, password);

            var request = new PutAttachmentRequest(id, "test1", HttpContentTypes.Text, byteArr);
            var client = new MyCouchClient(connString, database);
            var response = await client.Attachments.PutAsync(request);

            //Console.WriteLine(response.IsSuccess);
            //Console.WriteLine(response.Reason);
        }

        public static string Object2JsonString(Object o)
        {
            if (o == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.SerializeObject(o);
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public static async Task<bool> PostJson2Couch(string username, string password, string database, string documentId, object o)
        {
            string connString = SetupConnString(username, password);

            using (var client = new MyCouchClient(connString, database))
            {
                string jsonString = Object2JsonString(o);

                DocumentHeaderResponse response = await client.Documents.PutAsync(documentId, jsonString);

                if (!response.IsSuccess)
                {
                    return false;
                }

                return true;
            }
        }

        public static void ByteArr2File(byte[] byteArr, string fileOutPath)
        {
            if (byteArr == null)
            {
                return;
            }

            try
            {
                File.WriteAllBytes(fileOutPath, byteArr);
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public static byte[] File2ByteArr(string filepath)
        {
            if (string.IsNullOrEmpty(filepath))
            {
                return null;
            }

            try
            {
                return File.ReadAllBytes(filepath);
            }
            catch (Exception ex)
            {
                return null;
            }
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
