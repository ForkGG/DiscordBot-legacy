using System;
using System.Data.SQLite;
using System.Net;

public class Server
    {
        public Server()
        {
            fullpath = System.IO.Path.Combine(location, filename);
            connectionstr = string.Format("Data Source = {0}", fullpath);
        }

        private string location = AppDomain.CurrentDomain.BaseDirectory;
        private string filename = "database.db";
        private string fullpath;
        public string connectionstr;


        public void UpdateLive(string Videoid, bool Value)
        {
            using (var sqlconn = new SQLiteConnection(connectionstr))
            {
                string insert = "UPDATE YTLives SET Posted=@posted WHERE URL=@url";
                var cmd = new SQLiteCommand(insert, sqlconn);
                cmd.Parameters.AddWithValue("@url", Videoid);
                cmd.Parameters.AddWithValue("@Posted", Value.ToString());
                sqlconn.Open();
                cmd.ExecuteNonQuery();
            }
        }

    public void RemoveOnhold(string token)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "DELETE FROM Onhold WHERE Token=@token";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@token", token);
            sqlconn.Open();
            cmd.ExecuteNonQuery();
        }
    }
    /// <summary>Check if token and ip are same
    /// </summary>
    public object CheckAuth2(string token, string ip)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM Auth WHERE Token=@token AND IP=@ip";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@ip", ip);
            sqlconn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
    /// <summary>Check if ip is in the database, if so return us the token, num 0 checks if exist (bool), num 1 returns the token
    /// </summary>
    public object CheckIfIPExist(string ip, int num)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM Auth WHERE IP = @ip";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@ip", ip);
            sqlconn.Open();
            if (num == 0) {

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                
            } else if (num == 1)
            {
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return (string)reader["Token"];
                    }
                }
            }
            return null;
        }
    }
    /// <summary>Check if token or serverid exists in the database
    /// </summary>
    public object CheckAuth(string token,ulong serverid = 0)
        {
            using (var sqlconn = new SQLiteConnection(connectionstr))
            {
                string insert = "SELECT * FROM Auth WHERE Serverid=@serverid OR Token=@token";
                var cmd = new SQLiteCommand(insert, sqlconn);
                cmd.Parameters.AddWithValue("@serverid", serverid);
                cmd.Parameters.AddWithValue("@token", token);
                sqlconn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }
    public object CheckOnhold(string token,string ip = "Null")
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM Onhold WHERE Token=@token OR IP = @ip";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@ip", ip);
            sqlconn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
    public void InsertAuth(ulong serverid, string token,string IP)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "INSERT INTO Auth(Serverid,Token,IP) VALUES (@serverid,@token,@ip)";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@ip", IP);
            sqlconn.Open();
            cmd.ExecuteNonQuery();
        }
    }
    public void InsertRole(ulong serverid, ulong roleid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "INSERT INTO Role(Serverid,Roleid) VALUES (@serverid,@roleid)";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
            cmd.Parameters.AddWithValue("@roleid", roleid);
            sqlconn.Open();
            cmd.ExecuteNonQuery();
        }
    }
    public void RemoveRole(ulong serverid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "DELETE FROM Role WHERE Serverid = @serverid";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
            sqlconn.Open();
            cmd.ExecuteNonQuery();
        }
    }
    public string GetTokenOfServer(ulong serverid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM Auth WHERE Serverid=@serverid";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
            sqlconn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    return (string)reader["Token"];
                }

            }
        }
        return null;
    }
    /// <summary>Int 1 is for Onhold database, Int 2 is for Auth database
    /// </summary>
    public string GetIPForToken(string token,int db)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string dbname = null;
            if (db == 1) { dbname = "Onhold"; } else { dbname = "Auth"; }
            string insert = $"SELECT * FROM {dbname} WHERE Token=@token";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@token", token);
            sqlconn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    return (string)reader["IP"];
                }

            }
        }
        return null;
    }
    public object GetRole(ulong Serverid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM Role WHERE Serverid=@Serverid";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@Serverid", Serverid);
            sqlconn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    return reader["Roleid"];
                }

            }
        }
        return 0;
    }
    public void InsertOnhold(string token, string IP)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "INSERT INTO Onhold(Token,IP) VALUES (@token,@ip)";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@token", token);
            cmd.Parameters.AddWithValue("@ip", IP);
            sqlconn.Open();
            cmd.ExecuteNonQuery();
        }
    }
    public void Check(string videoid, bool value)
        {
            using (var sqlconn = new SQLiteConnection(connectionstr))
            {
                string insert = "INSERT INTO YTVideos(URL,Posted) VALUES (@url,@posted)";
                var cmd = new SQLiteCommand(insert, sqlconn);
                cmd.Parameters.AddWithValue("@url", videoid);
                cmd.Parameters.AddWithValue("@posted", value.ToString());
                sqlconn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    public void RemoveAuth(ulong serverid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "DELETE FROM Auth WHERE Serverid = @serverid";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
            sqlconn.Open();
            cmd.ExecuteNonQuery();
        }
    }
    }
