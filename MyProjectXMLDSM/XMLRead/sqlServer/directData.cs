using System;
using System.Data.SqlClient;
using System.Security.Cryptography;

namespace sqleasy
{
    class SqlDi
    {
      
        /// <summary>
        /// string connectionString = @"Server=DESKTOP-ANOGUR9\WINCCPLUSMIG2014;Database=dbExamples;Integrated Security=True;";
        /// </summary>
        
        #region VARIABELs
        public string m_conStr;
        public string m_user;
        public string m_pass;
        #endregion
        
        public SqlDi(string conStr, string user, string pass)
        {
            this.m_conStr = conStr;
            this.m_user = user;
            this.m_pass = pass;
        }


        #region CONTROLs
        public bool csCValidFromSQL()
        {
            string connectionString = @"Server=DESKTOP-ANOGUR9\WINCCPLUSMIG2014;Database=dbExamples;Integrated Security=True;";
            string query = "SELECT TOP (1000) [name] FROM [dbo].[user]";

            bool result = false;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string name = reader["name"].ToString();
                                if(name is null)
                                {
                                    Console.WriteLine("Error from SQL - name is empty ! ");
                                    return false;
                                }
                                else
                                {
                                    Console.WriteLine(name);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred: " + ex.Message);
                }
            }
            return true;
        }
        #endregion
    }
}