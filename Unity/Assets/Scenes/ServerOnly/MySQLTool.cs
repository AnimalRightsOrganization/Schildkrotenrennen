using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using UnityEngine;
using MySql.Data.MySqlClient;

namespace NetCoreServer.Utils
{
    public class MySQLTool : MonoBehaviour
    {
        protected static MySqlConnectionStringBuilder builder
        {
            get
            {
                var bd = new MySqlConnectionStringBuilder
                {
                    Server = "localhost",
                    Database = "db_user",
                    UserID = "seol",
                    Password = "123456",
                    SslMode = MySqlSslMode.None,
                };
                return bd;
            }
        }


        public static async void OnGetUserInfo(string usr, string pwd)
        {
            UserInfo result = (await GetUserInfo(usr, pwd));
        }
        public static async Task<UserInfo> GetUserInfo(string usr, string pwd)
        {
            UserInfo userInfo = new UserInfo();

            string SQL = $"SELECT * FROM tb_user WHERE username='{usr}' AND password='{pwd}';";
            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = SQL;

                    using (DbDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            userInfo.userid = reader.GetInt32("userid");
                            userInfo.username = reader.GetString("username");
                            userInfo.nickname = reader.GetString("nickname");
                            userInfo.score = reader.GetInt32("score");
                            return userInfo;
                        }
                    }
                }
                //Debug.Log("Closing connection");
                return null;
            }
        }
        public static async Task<UserInfo> GetUserInfo(string token)
        {
            UserInfo userInfo = new UserInfo();

            string SQL = $"SELECT * FROM tb_user WHERE token='{token}';";
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
                            userInfo.userid = reader.GetInt32("userid");
                            userInfo.username = reader.GetString("username");
                            userInfo.nickname = reader.GetString("nickname");
                            userInfo.score = reader.GetInt32("score");
                            return userInfo;
                        }
                    }
                }
                //Debug.Log("Closing connection");
                return null;
            }
        }

        public static async Task<int> CheckUserExist(string usr, string pwd)
        {
            string SQL = $"SELECT COUNT(*) FROM tb_user WHERE username='{usr}';";
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

        public static async Task<bool> SignUp(string usr, string pwd)
        {
            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();
                //using (var command = new MySqlCommand(SQL, conn)) //不适合多语句
                using (var command = conn.CreateCommand())
                {
                    // 先通过查询确定主键不重复，再执行插入
                    string SQL1 = $"SELECT COUNT(*) FROM tb_user WHERE username='{usr}';";
                    command.CommandText = SQL1;
                    int count = Convert.ToInt32(command.ExecuteScalar());
                    Debug.Log($"检查用户名是否占用了，count={count}");
                    if (count > 0)
                    {
                        Debug.Log($"已经存在用户:{usr}");
                        return false;
                    }

                    // createtime默认:CURRENT_TIMESTAMP，创建数据时自动生成当前时间
                    string SQL2 = $"INSERT INTO tb_user (userid, username, nickname, password) VALUES ('{0}', '{usr}', '{usr}', '{pwd}');";
                    command.CommandText = SQL2;
                    int rowCount = await command.ExecuteNonQueryAsync();
                    Debug.Log($"Number of rows inserted={rowCount}"); //插入了几行
                    return rowCount == 1;
                }
            }
        }
    }
}