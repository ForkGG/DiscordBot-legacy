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

        public void UpdateVideo(string Videoid, bool Value)
        {
            using (var sqlconn = new SQLiteConnection(connectionstr))
            {
                string insert = "UPDATE YTVideos SET Posted=@posted WHERE URL=@url";
                var cmd = new SQLiteCommand(insert, sqlconn);
                cmd.Parameters.AddWithValue("@url", Videoid);
                cmd.Parameters.AddWithValue("@Posted", Value.ToString());
                sqlconn.Open();
                cmd.ExecuteNonQuery();
            }
        }

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

        public void removeexpiredorder(long serverid, long orderid)
        {
            using (var sqlconn = new SQLiteConnection(connectionstr))
            {
                string insert = "DELETE FROM OrdersList WHERE Serverid=@serverid AND orderpostid = @orderpostid";
                var cmd = new SQLiteCommand(insert, sqlconn);
                cmd.Parameters.AddWithValue("@serverid", serverid);
                cmd.Parameters.AddWithValue("@orderpostid", orderid);
                sqlconn.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public object checkifidexist(string videoid)
        {
            using (var sqlconn = new SQLiteConnection(connectionstr))
            {
                string insert = "SELECT * FROM YTVideos WHERE URL=@url";
                var cmd = new SQLiteCommand(insert, sqlconn);
                cmd.Parameters.AddWithValue("@url", videoid);
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

            return null;
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
    public object GetIPForToken(string token)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM Onhold WHERE Token=@token";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@token", token);
            sqlconn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {


                    return reader["IP"];
                }

            }
        }
        return null;
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

        public void removereports()
        {
            using (var sqlconn = new SQLiteConnection(connectionstr))
            {
                string insert = "DELETE FROM Reports";
                var cmd = new SQLiteCommand(insert, sqlconn);
                sqlconn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
