using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Helpers
{
   public class DapperHelper<T>
   {
      private readonly string _connectionString;

      public DapperHelper()
      {
         // Read from appsettings.json
         _connectionString = ConfigurationManager.ConnectionStrings["database"].ConnectionString;
      }
      //for insert
      public void Execute(string sql, T t)
      {
         IDbConnection db = new SqlConnection(_connectionString);
         /*
         var sql = "INSERT INTO Users (Name, Email) VALUES (@Name, @Email)";

         var user = new User
         {
             Name = "John Doe",
             Email = "john@email.com"
         };
         */

         db.Execute(sql, t);
      }

      //for update and delete
      public void Execute(string sql, object obj)
      {
         IDbConnection db = new SqlConnection(_connectionString);

         /*
         var sql = "UPDATE Users SET Name = @Name WHERE Id = @Id";

         db.Execute(sql, new { Id = 1, Name = "Jane Doe" });
         */

         db.Execute(sql, obj);
      }

      public T Get(string sql, object param)
      {
         //var sql = "SELECT * FROM Users WHERE Id = @Id";
         //param = new { Id = 1 };


         IDbConnection db = new SqlConnection(_connectionString);

         var user = db.QueryFirstOrDefault<T>(sql, param);

         //Console.WriteLine(user?.Name);

         return user;
      }

      public List<T> GetAll(string sql, object param)
      {
         //var sql = "SELECT * FROM Users WHERE Id = @Id";
         //param = new { Id = 1 };


         IDbConnection db = new SqlConnection(_connectionString);

         var list = db.Query<T>(sql, param).ToList();


         return list;
      }

   }


   public class DapperHelper2<T>
   {
      private string _connectionString;

      public DapperHelper2(string connectionString)
      {
         _connectionString = connectionString;
      }


      public T Get(string sql, object param)
      {


         IDbConnection db = new SqlConnection(_connectionString);

         var user = db.QueryFirstOrDefault<T>(sql, param);


         return user;
      }


   }
}
