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

    public void CreateIfNot()
    {
        Console.WriteLine("Checking for database...");
        if (!DBcheck(fullpath))
        {
            Console.WriteLine("There isnt any database, creating one...");
            string createtable = @"CREATE TABLE `Auth` (
	`ID`	INTEGER,
	`Serverid`	INTEGER,
	`Token`	TEXT,
	`IP`	TEXT,
	PRIMARY KEY(`ID` AUTOINCREMENT)
);
CREATE TABLE `OnJoin` (

    `ID`    INTEGER,
	`Serverid`  INTEGER,
	`Channelid` INTEGER,
	`Roleid`    INTEGER,
	PRIMARY KEY(`ID` AUTOINCREMENT)
);
CREATE TABLE `Onhold` (

    `ID`    INTEGER,
	`Token` TEXT,
	`IP`    TEXT,
	PRIMARY KEY(`ID` AUTOINCREMENT)
);
CREATE TABLE `Notify` (

    `ID`    INTEGER,
	`Serverid`  INTEGER,
	`Channelid` INTEGER,
	`Messageid` INTEGER,
	PRIMARY KEY(`ID` AUTOINCREMENT)
);
            ";
            using (SQLiteConnection sqlconn = new SQLiteConnection(connectionstr))
            {
                SQLiteCommand cmd = new SQLiteCommand(createtable, sqlconn);

                sqlconn.Open();
                cmd.ExecuteNonQuery();
            }
            Console.WriteLine("Created a new database");
        } else { Console.WriteLine("Database exists."); }
    }
    public void InsertNotify(ulong serverid, ulong channelid, ulong messageid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "INSERT INTO Notify(Serverid,Channelid,Messageid) VALUES (@serverid,@channelid,@messageid)";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
            cmd.Parameters.AddWithValue("@channelid", channelid);
            cmd.Parameters.AddWithValue("@messageid", messageid);
            sqlconn.Open();
            cmd.ExecuteNonQuery();
        }
    }
    public void UpdateNotify(ulong serverid, ulong channelid,ulong messageid)
        {
            using (var sqlconn = new SQLiteConnection(connectionstr))
            {
                string insert = "UPDATE Notify SET Channelid=@channelid,Messageid=@messageid WHERE Serverid=@serverid";
                var cmd = new SQLiteCommand(insert, sqlconn);
                cmd.Parameters.AddWithValue("@channelid", channelid);
                cmd.Parameters.AddWithValue("@serverid", serverid);
            cmd.Parameters.AddWithValue("@messageid", messageid);
            sqlconn.Open();
                cmd.ExecuteNonQuery();
            }
        }
    public void RemoveNotify(ulong serverid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "DELETE FROM Notify WHERE Serverid=@serverid";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
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
    /// <summary>Check if server have a subscribed to a channel notification
    /// </summary>
    public bool CheckIfSubscribed(ulong serverid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM Notify WHERE Serverid=@serverid";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
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
    /// <summary>Check if channel or role is created already
    /// </summary>
    public object CheckRoleAndChannel(ulong serverid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM OnJoin WHERE Serverid=@serverid";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
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
    public void InsertRole(ulong serverid, ulong roleid,ulong channelid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "INSERT INTO OnJoin(Serverid,Roleid,Channelid) VALUES (@serverid,@roleid,@channelid)";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@serverid", serverid);
            cmd.Parameters.AddWithValue("@roleid", roleid);
            cmd.Parameters.AddWithValue("@channelid", channelid);
            sqlconn.Open();
            cmd.ExecuteNonQuery();
        }
    }
    public void RemoveRole(ulong serverid)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "DELETE FROM OnJoin WHERE Serverid = @serverid";
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
    public ulong GetServerForToken(string token)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM Auth WHERE Token=@token";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@token", token);
            sqlconn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    return (ulong)reader["Serverid"];
                }

            }
        }
        return 0;
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
    /// <summary>Int 0 returns Roleid , Int 1 Returns Channelid
    /// </summary>
    public long GetRoleandChannel(ulong Serverid, int num = 0)
    {
        using (var sqlconn = new SQLiteConnection(connectionstr))
        {
            string insert = "SELECT * FROM OnJoin WHERE Serverid=@Serverid";
            var cmd = new SQLiteCommand(insert, sqlconn);
            cmd.Parameters.AddWithValue("@Serverid", Serverid);
            sqlconn.Open();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    switch (num)
                    {
                        case 0:
                            return (long)reader["Roleid"];
                        case 1:
                            return (long)reader["Channelid"];
                    }
                   
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
    public bool DBcheck(string fullpath)
    {
        return System.IO.File.Exists(fullpath);
    }
}
