using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MySqlConnector;

namespace NetCoreServer.Utils
{
    public class MySQLTool
    {
        //const string connectionString = "localhost";
        //protected static MySqlConnection client;
        protected static MySqlConnectionStringBuilder builder
        {
            get
            {
                var bd = new MySqlConnectionStringBuilder
                {
                    Server = "localhost",
                    Database = "turtle",
                    UserID = "root",
                    Password = "",
                    SslMode = MySqlSslMode.None,
                };
                return bd;
            }
        }

        public static void Connect()
        {
            //client = new MySqlConnection(connectionString);
        }
        public static void Insert()
        {
            //一次插入多条
            //string SQL = $"INSERT INTO db_user (userid, username, password) VALUES ('{1}', '{usr}', '{pwd}'), ('{2}', 'apple', 'applepwd');";
        }
        public static void Delete() { }
        public static void Update() { }
        public static void Query() { }

        public static async Task Main()
        {
            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                Debug.Print("Opening connection");
                await conn.OpenAsync();

                //Debug.Print("using command");
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "DROP TABLE IF EXISTS inventory;";
                    await command.ExecuteNonQueryAsync();
                    Debug.Print("Finished dropping table (if existed)");

                    command.CommandText = "CREATE TABLE inventory (id serial PRIMARY KEY, name VARCHAR(50), quantity INTEGER);";
                    await command.ExecuteNonQueryAsync();
                    Debug.Print("Finished creating table");

                    command.CommandText = @"INSERT INTO inventory (name, quantity) VALUES (@name1, @quantity1),
                        (@name2, @quantity2), (@name3, @quantity3);";
                    command.Parameters.AddWithValue("@name1", "banana");
                    command.Parameters.AddWithValue("@quantity1", 150);
                    command.Parameters.AddWithValue("@name2", "orange");
                    command.Parameters.AddWithValue("@quantity2", 154);
                    command.Parameters.AddWithValue("@name3", "apple");
                    command.Parameters.AddWithValue("@quantity3", 100);

                    int rowCount = await command.ExecuteNonQueryAsync();
                    Debug.Print(String.Format("Number of rows inserted={0}", rowCount));
                }

                // connection will be closed by the 'using' block
                Debug.Print("Closing connection");
            }
        }
        public static async Task TestQuery()
        {
            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                Debug.Print("Opening connection");
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM inventory;";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Debug.Print(string.Format(
                                "Reading from table=({0}, {1}, {2})",
                                reader.GetInt32(0),
                                reader.GetString(1),
                                reader.GetInt32(2)));
                        }
                    }
                }

                Debug.Print("Closing connection");
            }

            Debug.Print("Press RETURN to exit");
        }

        public static async Task<bool> CheckLogin(string usr, string pwd)
        {
            string SQL = $"SELECT * FROM db_user WHERE username='{usr}';";
            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQL;

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            //TODO: 预防键或值为空
                            //var userid = reader.GetInt32("userid");
                            //Debug.Print($"userid={userid}");
                            //var nickname = reader.GetString("nickname");
                            //Debug.Print($"nickname={nickname}");
                            //var createtime = reader.GetDateTime("createtime");
                            //Debug.Print($"createtime={createtime}");
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public static async Task<int> QueryAll()
        {
            // 查询全服玩家数
            string SQL = "SELECT COUNT(*) FROM db_user";
            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(SQL, conn))
                {
                    int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return count;
                }
            }
        }
        public static async Task<int> CheckUserExist(string usr, string pwd)
        {
            string SQL = $"SELECT COUNT(*) FROM db_user WHERE username='{usr}';";
            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQL;
                    int count = Convert.ToInt32(command.ExecuteScalarAsync());
                    return count;
                }
            }
        }
        public static async Task<bool> Register(string usr, string pwd)
        {
            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();
                //using (var command = new MySqlCommand(SQL, conn)) //不适合多语句
                using (var command = conn.CreateCommand())
                {
                    // 先通过查询确定主键不重复，再执行插入
                    string SQL1 = $"SELECT COUNT(*) FROM db_user WHERE username='{usr}';";
                    command.CommandText = SQL1;
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    Debug.Print($"count={count}");
                    if (count > 0)
                    {
                        Debug.Print($"已经存在用户:{usr}");
                        return false;
                    }

                    // createtime默认:CURRENT_TIMESTAMP，创建数据时自动生成当前时间
                    string SQL2 = $"INSERT INTO db_user (userid, username, password) VALUES ('{0}', '{usr}', '{pwd}');";
                    command.CommandText = SQL2;
                    int rowCount = await command.ExecuteNonQueryAsync();
                    Debug.Print($"Number of rows inserted={rowCount}"); //插入了几行
                    return rowCount == 1;
                }
                Debug.Print("Closing connection");
            }
        }
    }
}