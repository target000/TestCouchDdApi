using LoveSeat;
using MyCouch;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

            string username = "root";
            string password = "7";
            string database = "t1";

            Post2CouchDB(username, password, database);
            Console.ReadKey();
        }

        public static async void Post2CouchDB(string username, string password, string database)
        {
            string connString = string.Format("http://{0}:{1}@localhost:5984", username, password);
            Console.WriteLine(connString);

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
                s.Other = " wow this whole newtonsoft thign is actually prtty alwesome";
                s.HerBook = b;

                string jsonString = JsonConvert.SerializeObject(s);

                try
                {
                    await client.Documents.PutAsync("68e56facc00392d9a35dde468600915f", "1-96254bdaf067b68dde927a3f386e9a55", jsonString);
                }
                catch (Exception)
                {
                    throw;
                }
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
