using System;
using Mongo.Data;

namespace MongoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            //MProjects.InsertDefaul();
            //MProjects.TestFilter();
            //MProjects.TestUpdateNestedField();
            //MProjects.NestedArrrayAddItem();
            //MProjects.NestedArrrayRemoveItem();
            MProjects.TestAddSlice();






            //var projectName = "Demo project 2";
            //var taskName = "Demo Task 1";
            //
            //MProjects.InsertAnswer(projectName, taskName, "questions2", "ivanz@meta.ua", "4");
            //MProjects.InsertAnswer(projectName, taskName, "questions1", "ivanz@meta.ua", "7");
            //MProjects.InsertAnswer("Demo project 3", taskName, "questions1", "ivanz@meta.ua", "4");

            //var userService = new UserService();

            //var userRole = new Role
            //{
            //    RoleName = "user",
            //    Priority = 1
            //};
            //var adminRole = new Role
            //{
            //    RoleName = "admin",
            //    Priority = 5
            //};
            //for (int i = 0; i < 10; i++)
            //{
            //    userService.Create(new TestUser()
            //    {
            //        Roles = (i % 5 == 0) ? new List<Role> { userRole, adminRole } : new List<Role> { userRole },  
            //        Age = 18,
            //        FirstName = "FName00" + i.ToString("D5"),
            //        LastName = "LName" + i
            //    });
            //}

            //userService.GetAll().ToList().ForEach(x =>
            //{
            //    x.Age += 5;
            //    userService.Update(x);
            //});
           
            //var users = userService.GetGamesDetails(10, 0);//.ForEach(x => Console.WriteLine(x));
            //foreach (var testUser in users)
            //{
            //    Console.WriteLine(testUser.ToJson());
            //    //Console.WriteLine(testUser.FirstName + ' ' + testUser.LastName + ' ' + testUser.Age + ' ' + testUser.Roles.Count);
            //    userService.Delete(testUser.Id.ToString());
            //}
            Console.ReadKey();
        }
    }
}
