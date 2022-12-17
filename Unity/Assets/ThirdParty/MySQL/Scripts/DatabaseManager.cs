#if UNITY_SERVER || UNITY_EDITOR
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace DatabaseEssential
{
    public class DatabaseManager
    {
        static MySqlConnection con;

        /// <summary>
        /// Method to Open/Initialize connection with Server, with return type of bool.
        /// </summary>
        /// <returns></returns>
        public static bool Initialize()
        {
            var config = new SQLConfiguration
            {
                host = "localhost",
                database = "moefight",
                user = "seol",
                password = "123456",
            };
            string connectionString = $"Server={config.host};" +
                $"Database={config.database};" +
                $"User={config.user};" +
                $"password={config.password};";

            // Connect to Server using the connectionString and return true means connection stabled
            try
            {
                con = new MySqlConnection(connectionString);
                con.Open();
                return true;
            }
            // Handle errors based on commond error numbers. 
            // 0: Cannot connect to server.
            // 1045: Invalid Username and/or Password.
            catch (MySqlException e)
            {
                switch (e.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot Connect to Server. Contact Administrator.");
                        break;
                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again.");
                        break;
                    default:
                        UnityEngine.Debug.Log($"err: {e.Number}, {e.Message}");
                        break;
                }
                return false;
            }
        }
        /// <summary>
        /// Method to Close connection from Server.
        /// </summary>
        /// <returns></returns>
        public static bool Close()
        {
            try
            {
                con.Close();
                return true;
            }
            catch (MySqlException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }
        /// <summary>
        /// Method to execute any Reader SQL Statement
        /// </summary>
        /// <param name="tableName">Name of the Table</param>
        /// <param name="statement">SQL Statement</param>
        /// <returns></returns>
        public static string Query(string statement)
        {
            string result = "";
            if (Initialize() == true)
            {
                MySqlCommand cmd = new MySqlCommand(); //MySQL Command
                cmd.CommandText = statement; //Assign the query to MySQLCommand 
                cmd.Connection = con; // Assign the connection to SQLCommand Connection

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            result += reader.GetValue(i).ToString() + "-";
                        }
                        result += "/n";
                    }
                }

                Close(); //Closing the Connection

                return result;
            }
            else
            {
                return result; //Just return a empty string on failure
            }
        }
        /// <summary>
        /// Method to execute Non-Query SQL Command
        /// </summary>
        /// <param name="statement"></param>
        public static void NonQuery(string statement)
        {
            if (Initialize() == true)
            {
                MySqlCommand cmd = new MySqlCommand(); //MySQL Command
                cmd.CommandText = statement; //Assign the query to MySQLCommand 
                cmd.Connection = con; // Assign the connection to SQLCommand Connection

                cmd.ExecuteNonQuery(); //Execute query (ExecuteNonQuery will not return any data)

                Close(); //Closing the Connection
            }
        }

        /// <summary>
        /// Method to Insert Record(s) to Table
        /// </summary>
        /// <param name="tableName"> Name of the table</param>
        /// <param name="keys"> Key(s) name or Column(s) Name</param>
        /// <param name="values"> Value(s) of Key(s) or Column(s) Name</param>
        public static void InsertRecord(string tableName, string keys, string values)
        {
            string query = $"INSERT INTO {tableName} ({keys}) VALUES ({values})";

            if (Initialize() == true)
            {
                MySqlCommand cmd = new MySqlCommand(); // MySQL Command
                cmd.CommandText = query; // Assign the query to MySQLCommand
                cmd.Connection = con; // Assign the connection to SQLCommand Connection

                cmd.ExecuteNonQuery(); //Execute query (ExecuteNonQuery will not return any data)

                Close(); //Closing the connection
            }
        }
        /// <summary>
        /// Method to Delete Record(s) from Table
        /// </summary>
        /// <param name="tableName">Name of the Table</param>
        /// <param name="condition">WHERE Condition</param>
        public static void DeleteRecord(string tableName, string condition)
        {
            string query = $"DELETE FROM {tableName} WHERE {condition}";
            // Opening the connection
            if (Initialize() == true)
            {
                MySqlCommand cmd = new MySqlCommand();  // MySQL Command  
                cmd.CommandText = query; // Assign the query to MySQLCommand
                cmd.Connection = con; // Assign the connection to SQLCommand Connection

                cmd.ExecuteNonQuery(); //Execute query (ExecuteNonQuery will not return any data)

                Close(); //Closing the connection
            }
        }
        /// <summary>
        /// Method to Updata Data in Database
        /// </summary>
        /// <param name="tableName">Name of the Table</param>
        /// <param name="condition">SET Condition, also WHERE condition too. Example: Username='Ashikur Rahman' WHERE Username='Srejon Khan'</param>
        public static void UpdateRecord(string tableName, string condition)
        {
            string query = $"UPDATE {tableName} SET {condition}";
            // Opening the connection
            if (Initialize() == true)
            {
                MySqlCommand cmd = new MySqlCommand(); //MySQL Command
                cmd.CommandText = query; //Assign the query to MySQLCommand
                cmd.Connection = con; // Assign the connection to SQLCommand Connection

                cmd.ExecuteNonQuery(); //Execute query (ExecuteNonQuery will not return any data)

                Close(); //Closing the connection
            }
        }
        /// <summary>
        /// Method to Fetch All Records from Table according to column name
        /// </summary>
        /// <param name="tableName">Name of the Table</param>
        /// <param name="columnName">Name of Column(s) to fetch values</param>
        /// <returns></returns>
        public static List<string>[] SelectAllRecord(string tableName, string columnName)
        {
            string query = $"Select * FROM {tableName}";

            string[] columns = columnName.Split(',');
            List<string>[] results = new List<string>[columns.Length];
            for (int i = 0; i < columns.Length; i++)
            {
                results[i] = new List<string>();
            }

            //Opening Connection
            if (Initialize() == true)
            {
                MySqlCommand cmd = new MySqlCommand(); //MySQL Command
                cmd.CommandText = query; //Assign the query to MySQLCommand               
                cmd.Connection = con; // Assign the connection to SQLCommand Connection

                MySqlDataReader dataReader = cmd.ExecuteReader(); // Declaring a DataReader and executing the command

                while (dataReader.Read())
                {
                    for (int i = 0; i < columns.Length; i++)
                    {
                        results[i].Add(dataReader[columns[i].ToString()] + "");
                    }
                }

                dataReader.Close(); // Closing DataReader

                Close(); //Closing the Connection

                return results;
            }
            else
            {
                return results; //Just return a empty list on failure
            }
        }
        /// <summary>
        /// Method to count records of Table
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static int Count(string query)
        {
            //string query = $"SELECT Count(*) FROM {tableName}";
            int count = -1; // Negative value will represent the failure of connection

            //Opening the Connection
            if (Initialize() == true)
            {
                MySqlCommand cmd = new MySqlCommand(); //MySQL Command
                cmd.CommandText = query; //Assign the query to MySQLCommand
                cmd.Connection = con; // Assign the connection to SQLCommand Connection

                count = int.Parse(cmd.ExecuteScalar() + ""); //ExecuteScaler will return one value

                Close(); //Closing the connection

                return count;
            }
            else
            {
                return count;
            }
        }
    }

    public class SQLConfiguration
    {
        public string host, database, user, password;
    }
}
#endif