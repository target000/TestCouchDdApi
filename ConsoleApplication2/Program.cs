using MyCouch;
using MyCouch.Net;
using MyCouch.Requests;
using MyCouch.Responses;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AppBridgeMyCouchTest
{
    class MyCouchTest
    {
        private string Username { get; set; }
        private string Password { get; set; }
        private string ConnString { get; set; }

        private string filepath = @"C:\Users\xlu.APPBRIDGE\Desktop\db_comp.pdf";

        private string fileOutPath = @"C:\Users\xlu.APPBRIDGE\Desktop\nice_crap.pdf";

        public MyCouchTest()
        {
            Username = "root";
            Password = "111111";

            InitializeConnString();
        }

        public MyCouchTest(string username, string password)
        {
            Username = username;
            Password = password;

            InitializeConnString();
        }

        public void InitializeConnString()
        {
            ConnString = SetupConnString(Username, Password);
        }

        static void Main(string[] args)
        {

            string test1 = "";
            Console.WriteLine(string.IsNullOrEmpty(test1));

            Test();

            Console.WriteLine("End of program!");
            Console.ReadKey();
        }

        public static object GenerateTestObject()
        {
            Book b = new Book();
            b.Name = "This show it is updated";
            b.Author = "me";
            b.Page = 1;
            b.Type = "Fantasy Novel";

            Student s = new Student();
            s.Name = "for the first time ever to get this work";
            s.Gpa = 80;
            s.Description = "this is gonna be anwesome description that i post to the database awesome";
            s.Other = "wow this whole newtonsoft thign is actually pretty alwesome";
            s.HerBook = b;

            return s;
        }

        public static async void Test()
        {
            // for db access
            string documentID = "xyz";
            string username = "root";
            string password = "111111";
            string database = "stuff";

            //object o = GenerateTestObject();

            //string jsonString = JsonConvert.SerializeObject(o);

            var stuf = await GetDatabases(username, password);
            Console.WriteLine(stuf);
        }

        public static async Task<bool> PostJson2Couch2(string username, string password, string database, string jsonString)
        {
            string connString = SetupConnString(username, password);

            using (var store = new MyCouchStore(connString, database))
            {
                try
                {
                    var docHeader = await store.StoreAsync(jsonString);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public static async Task<IList<string>> GetDatabases(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(username))
            {
                return null;
            }

            string connString = SetupConnString(username, password);
            const string getAllDatabases = @"/_all_dbs";

            using (var client = new MyCouchServerClient(connString))
            {
                HttpRequest httpRequest = new HttpRequest(HttpMethod.Get, getAllDatabases);

                var response = await client.Connection.SendAsync(httpRequest);
                string content = await response.Content.ReadAsStringAsync();

                IList<string> dbNames = JsonConvert.DeserializeObject<IList<string>>(content);

                // remove the ones that are auto generated
                const string globalChanges = "_global_changes";
                const string metadata = "_metadata";
                const string replicator = "_replicator";
                const string users = "_users";

                dbNames.Remove(globalChanges);
                dbNames.Remove(metadata);
                dbNames.Remove(replicator);
                dbNames.Remove(users);

                return dbNames;
            }
        }

        private static string SetupConnString(string username, string password, string ipAddress = "localhost", int port = 5984)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            return string.Format("http://{0}:{1}@{2}:{3}", username, password, ipAddress, port);
        }

        public static async Task<byte[]> GetAttachmentFromCouch(string documentId, string attachmentName, string username, string password, string database)
        {
            string connString = SetupConnString(username, password);

            var client = new MyCouchClient(connString, database);
            var response = await client.Attachments.GetAsync(documentId, attachmentName);

            return response.Content;
        }

        /// <summary>
        /// This method will push the attachment to a document in the db
        /// </summary>
        /// <param name="byteArr">file byte array representation</param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="database"></param>
        /// <param name="documentId">the document</param>
        public static async void PostAttachment2Couch(byte[] byteArr, string username, string password, string database, string documentId)
        {
            string connString = SetupConnString(username, password);

            var request = new PutAttachmentRequest(documentId, "test1", HttpContentTypes.Text, byteArr);
            var client = new MyCouchClient(connString, database);
            var response = await client.Attachments.PutAsync(request);
        }

        // note this method can directly push object up
        public static async Task<bool> PostEntity2Couch(string username, string password, string database, string documentId, object o)
        {
            string connString = SetupConnString(username, password);

            using (var client = new MyCouchClient(connString, database))
            {
                var response = await client.Entities.PutAsync(o);

                if (!response.IsSuccess)
                {
                    return false;
                }

                return true;
            }
        }

        /// <summary>
        /// This method will push json string up to the CouchDb database
        /// </summary>
        /// <param name="username">username to access the db</param>
        /// <param name="password">password to access the db</param>
        /// <param name="database">name of the db</param>
        /// <param name="documentId">the document within the db</param>
        /// <param name="o">Input object to be pushed up to the database</param>
        /// <returns></returns>
        public static async Task<bool> PostJson2Couch(string username, string password, string database, string documentId, string jsonString)
        {
            string connString = SetupConnString(username, password);

            using (var client = new MyCouchClient(connString, database))
            {
                DocumentHeaderResponse response = await client.Documents.PutAsync(documentId, jsonString);

                if (!response.IsSuccess)
                {
                    return false;
                }

                return true;
            }
        }



        /// <summary>
        /// Convert an object to json string
        /// </summary>
        /// <param name="o">Any object</param>
        /// <returns>Json formatted string</returns>
        public static string Object2JsonString(object o)
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

        /// <summary>
        /// File.ReadAllBytes() method will be subject to 4.2 GB limit
        /// The default max size of attachment is 4 GB which can be changed to a larger value in the Couchdb config file
        /// </summary>
        /// <param name="filepath">the file path of the document</param>
        /// <returns>byte array counterpart of the original file</returns>
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

        /// <summary>
        /// Convert a byte array to a file
        /// </summary>
        /// <param name="byteArr">byte array to be converted</param>
        /// <param name="fileOutPath">the output file path of the file converted from the byte array</param>
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
