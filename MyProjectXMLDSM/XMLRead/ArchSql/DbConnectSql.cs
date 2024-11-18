using System;
using System.Text;
using Dsmdb;
using System.Data.SqlClient;
using Microsoft.EntityFrameworkCore;


namespace Archive
{
    public class ArchiveDbContext : DbContext
    {
        #region Tables
        public DbSet<dbUser> dbUser { get; set; }             
        #endregion

        public string StringConnectSqlDb {get; private set; }

        public ArchiveDbContext(string sTringConnec)
        {
            StringConnectSqlDb = sTringConnec;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);

            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(StringConnectSqlDb);
            }
        }

        #region MAKE_TABLE and CHECK CONNECT
        public void MakeTable(
            string tableName, 
            string nameOP, 
            List<string> nameSteps,         // Name of Steps                <----
            List<string> statusSteps,       // Status for specific Steps
            List<string> nameVariables,     // Name for Variabls            <----
            List<string> variables,         // Value of variabels       
            List<string> varMin,            // Min for a Variabel   
            List<string> varMax,            // Max for a Variabels
            List<string> nameComponent,     // Name of Component            <----
            List<string> components)        // Serial for Component
            
            // I have to add a Colum for Date and maybe ESTRC for each controls
        {
            // SQL query to check if the table exists
            var checkTableExistsQuery = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{tableName}'";

            try
            {
                // Check if the table exists
                int tableCount = Database.ExecuteSqlRaw($"SELECT CASE WHEN EXISTS({checkTableExistsQuery}) THEN 1 ELSE 0 END");

                if (tableCount > 0)
                {
                    Console.WriteLine($"Table '{tableName}' already exists. No action taken.");
                    return; // Exit the method if the table already exists
                }

                // Building the SQL query for table creation
                var createTableQuery = new StringBuilder();
                createTableQuery.AppendLine($"CREATE TABLE {tableName} (");
                createTableQuery.AppendLine($"{nameOP} NVARCHAR(MAX),");

                // Adding columns for steps and their status
                for (int i = 0; i < nameSteps.Count; i++)
                {
                    createTableQuery.AppendLine($"[{nameSteps[i]}] NVARCHAR(MAX),");
                    createTableQuery.AppendLine($"[{statusSteps[i]}] NVARCHAR(MAX),");
                }

                // Adding columns for variables, min, and max values
                for (int i = 0; i < nameVariables.Count; i++)
                {
                    createTableQuery.AppendLine($"[{nameVariables[i]}] NVARCHAR(MAX),");
                    createTableQuery.AppendLine($"[{variables[i]}] NVARCHAR(MAX),");
                    createTableQuery.AppendLine($"[{varMin[i]}] NVARCHAR(MAX),");
                    createTableQuery.AppendLine($"[{varMax[i]}] NVARCHAR(MAX),");
                }

                // Adding columns for components and their values
                for (int i = 0; i < nameComponent.Count; i++)
                {
                    createTableQuery.AppendLine($"[{nameComponent[i]}] NVARCHAR(MAX),");
                    createTableQuery.AppendLine($"[{components[i]}] NVARCHAR(MAX),");
                }

                // Date and Time
                createTableQuery.AppendLine($"{"DateAndTime"} NVARCHAR(MAX),");

                // Removing the last comma and closing the table definition
                createTableQuery.Length--; // Remove the last comma
                createTableQuery.AppendLine(");");

                // Execute the SQL command using Entity Framework's Database API
                Database.ExecuteSqlRaw(createTableQuery.ToString());
                Console.WriteLine("Table created successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating table: {ex.Message}");
            }


            // How to us my method :

            // var tableName = "MyDynamicTable";
            // var nameOP = "OperationName";
            // var nameSteps = new List<string> { "Step1", "Step2" };
            // var statusSteps = new List<string> { "Status1", "Status2" };
            // var nameVariables = new List<string> { "Var1", "Var2" };
            // var variables = new List<string> { "Val1", "Val2" };
            // var varMin = new List<string> { "Min1", "Min2" };
            // var varMax = new List<string> { "Max1", "Max2" };
            // var nameComponent = new List<string> { "Component1", "Component2" };
            // var components = new List<string> { "CompVal1", "CompVal2" };

            // .MakeTable(tableName, nameOP, nameSteps, statusSteps, nameVariables, variables, varMin, varMax, nameComponent, components);

            // When you can use it outside 
            // Execute the SQL command using Entity Framework's DbContext
            // using (var context = new YourDbContext())
            // {
            //     context.Database.ExecuteSqlRaw(createTableQuery.ToString());
            // }
        }

            public bool CheckDatabaseConnection()       // Check connection 
            {
                try
                {
                    return Database.CanConnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Database connection failed: {ex.Message}");
                    return false;
                }
            }

            public (bool IsConnected, string ErrorMessage) CheckDatabaseConnection_nextGen() // Check connection but the function make a more information
            {
                try
                {
                    if (Database.CanConnect())
                    {
                        return (true, "Connection successful.");
                    }
                    else
                    {
                        return (false, "Connection unsuccessful. Check if the database is running and accessible.");
                    }
                }
                catch (SqlException sqlEx)
                {
                    return (false, $"SQL Exception: {sqlEx.Message}");
                }
                catch (InvalidOperationException opEx)
                {
                    return (false, $"Invalid Operation: {opEx.Message}");
                }
                catch (Exception ex)
                {
                    return (false, $"General Error: {ex.Message}");
                }
            }

            #endregion
        
    }
}