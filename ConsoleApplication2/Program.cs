using MyCouch;
using MyCouch.Net;
using MyCouch.Requests;
using MyCouch.Responses;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        // ENTRY POINT
        static void Main(string[] args)
        {


            Test();

            Console.WriteLine("End of program!");
            Console.ReadKey();
        }

        public static async void Test()
        {
            // for db access
            string documentID = "68e56facc00392d9a35dde468600e9fd";
            string username = "root";
            string password = "111111";
            string dbname = "stuff";
            string documentRev = "1-94f74477074bf47e6b4d3222f78ce01b";

            //object o = GenerateTestObject();

            //string jsonString = JsonConvert.SerializeObject(o);

            //var stuf = await GetDatabase(username, password, dbname);
            //var stuf = await DatabaseExist(username, password, dbname);
            //var stuf = await DeleteDatabase(username, password, dbname);
            //var stuf = await CreateDatabase(username, password, dbname);
            //var stuf = await DeleteDocument(username, password, dbname, documentID, documentRev);

            var stuf = await GetDocumentRevision(username, password, dbname, documentID);

            Console.WriteLine(stuf);
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

        // maybe write a function for string generation 
        // /stuff/_all_docs 
        // try to map each command to a string
        // the command is a constant value or even maybe a enum


        public static async Task<IList<string>> GetDatabase(string username, string password, string databaseName)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(username))
            {
                return null;
            }

            string connString = SetupConnString(username, password);
            string getDatabase = string.Format(@"/{0}/_all_docs", databaseName);

            using (var client = new MyCouchServerClient(connString))
            {
                HttpRequest httpRequest = new HttpRequest(HttpMethod.Get, getDatabase);

                var response = await client.Connection.SendAsync(httpRequest);
                string content = await response.Content.ReadAsStringAsync();

                const string PROPERTY_ROWS = "rows";
                const string PROPERTY_ID = "id";

                JObject jObject = JObject.Parse(content);
                IList<string> fileIds = jObject[PROPERTY_ROWS].Select(t => (string)t[PROPERTY_ID]).ToList();

                return fileIds;
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

                // Removes ones generated by CouchDB by default
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

        public static async Task<bool> DatabaseExist(string username, string password, string databaseName)
        {
            string connString = SetupConnString(username, password);

            using (var client = new MyCouchClient(connString, databaseName))
            {
                GetDatabaseResponse response = await client.Database.GetAsync();

                if (!response.IsSuccess)
                {
                    return false;
                }

                return true;
            }
        }

        public static async Task<bool> DeleteDatabase(string username, string password, string databaseName)
        {
            string connString = SetupConnString(username, password);

            try
            {
                using (var client = new MyCouchServerClient(connString))
                {
                    await client.Databases.DeleteAsync(databaseName);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }

        public static async Task<bool> CreateDatabase(string username, string password, string databaseName)
        {
            string connString = SetupConnString(username, password);

            try
            {
                using (var client = new MyCouchServerClient(connString))
                {
                    await client.Databases.PutAsync(databaseName);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
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

        public static async Task<string> GetDocument(string username, string password, string database, string documentId)
        {
            string connString = SetupConnString(username, password);

            using (var client = new MyCouchClient(connString, database))
            {
                var response = await client.Documents.GetAsync(documentId);

                if (!response.IsSuccess)
                {
                    return null;
                }

                return response.Content;
            }
        }

        public static async Task<string> GetDocumentRevision(string username, string password, string database, string documentId)
        {
            string connString = SetupConnString(username, password);

            using (var client = new MyCouchClient(connString, database))
            {
                var response = await client.Documents.GetAsync(documentId);

                if (!response.IsSuccess)
                {
                    return null;
                }


                JObject jObject = JObject.Parse(response.Content);
                string revisionId = (string)jObject["_rev"];

                return revisionId;
            }
        }

        public static async Task<bool> DeleteDocument(string username, string password, string database, string documentId, string documentRev)
        {
            string connString = SetupConnString(username, password);

            using (var client = new MyCouchClient(connString, database))
            {
                DeleteDocumentRequest deleteDocument = new DeleteDocumentRequest(documentId, documentRev);
                DocumentHeaderResponse response = await client.Documents.DeleteAsync(deleteDocument);

                if (!response.IsSuccess)
                {
                    return false;
                }

                return true;
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
