using DAL.Interfaces;
using ModelsLibrary.Models.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repositories
{
	public class ResponseCodeRepository : IResponseCodeRepository
	{
		public List<ReponseCodeModel> GetAll()
		{
         SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["database"].ConnectionString);

         try
         {
            conn.Open();

            string query = @"
            select 
            [code]
            ,[name]
            ,[message]
            ,[created_date]
            from [response_code] 
            ";

            var parameters = new SqlParameter[] {
             //new SqlParameter("", )
         };

            SqlCommand cmd = new SqlCommand(query.ToString(), conn);
            cmd.Parameters.AddRange(parameters);
            cmd.CommandTimeout = 0;

            SqlDataReader dr = cmd.ExecuteReader();

            List<ReponseCodeModel> list = new List<ReponseCodeModel>();


            while (dr.Read())
               list.Add(LoadFromReader(dr));

            return list;
         }
         catch (Exception ex)
         {
            //LogWriter log = new LogWriter();
            //log.Error(ex);

            throw ex;
         }
         finally
         {
            conn.Close();
         }
      }

		public ReponseCodeModel GetByCode(int code)
		{
         SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["database"].ConnectionString);

         try
         {
            conn.Open();

            string query = @"
            select 
            [code]
            ,[name]
            ,[message]
            ,[created_date]
            from [response_code]
            where code=@code
            ";

            var parameters = new SqlParameter[] {
             new SqlParameter("code", code)
         };

            SqlCommand cmd = new SqlCommand(query.ToString(), conn);
            cmd.Parameters.AddRange(parameters);
            cmd.CommandTimeout = 0;

            SqlDataReader dr = cmd.ExecuteReader();

            ReponseCodeModel model = null;


            while (dr.Read())
            {
               model = (LoadFromReader(dr));
            }

            return model;
         }
         catch (Exception ex)
         {
            //LogWriter log = new LogWriter();
            //log.Error(ex);

            throw ex;
         }
         finally
         {
            conn.Close();
         }
      }

      private ReponseCodeModel LoadFromReader(SqlDataReader dr)
      {
         /*
          [code]
            ,[name]
            ,[message]
            ,[created_date]
          */
         var model = new ReponseCodeModel();
         model.Code = (int)dr["code"];
         model.Name = dr["name"].ToString();
         model.Message = dr["message"].ToString();
         model.CreatedDate = DateTime.Parse(dr["created_date"].ToString());

         return model;
      }
   }
}
