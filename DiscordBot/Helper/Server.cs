using Discord.Commands;
using System;
using System.Data.SQLite;
using System.IO;
using System.Net;

public class Server
{
    public Server()
    {
        fullpath = Path.Combine(location, filename);
        connectionstr = $"Data Source = {fullpath}";
    }

    private string location = AppDomain.CurrentDomain.BaseDirectory;
    private string filename = "database.db";
    private string fullpath;
    public string connectionstr;

    public void CreateIfNot()
    {
        Console.WriteLine("Checking for database...");
        if (!DbCheck(fullpath))
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
	`sevent`    INTEGER DEFAULT 1,
	`Messageid` INTEGER,
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
        }
        else
        {
            Console.WriteLine("Database exists.");
        }
    }

    public void InsertNotify(ulong serverid, ulong channelid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "INSERT INTO Notify(Serverid,Channelid) VALUES (@serverid,@channelid)";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        cmd.Parameters.AddWithValue("@channelid", channelid);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    public void UpdateNotify(ulong serverid, ulong channelid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "UPDATE Notify SET Channelid=@channelid WHERE Serverid=@serverid";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@channelid", channelid);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Update server event
    /// </summary>
    public void UpdateSEvent(ulong serverid, ulong messageid, int num)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "UPDATE OnJoin SET sevent=@sevent,Messageid=@messageid WHERE Serverid=@serverid";

        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        cmd.Parameters.AddWithValue("@sevent", num);
        cmd.Parameters.AddWithValue("@messageid", messageid);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    public void RemoveNotify(ulong serverid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "DELETE FROM Notify WHERE Serverid=@serverid";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    public void RemoveOnhold(string token)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "DELETE FROM Onhold WHERE Token=@token";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@token", token);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    /// <summary>
    /// Check if token and ip are same
    /// </summary>
    public bool CheckAuth2(string token, string ip)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM Auth WHERE Token=@token AND IP=@ip";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@token", token);
        cmd.Parameters.AddWithValue("@ip", ip);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.HasRows;
    }

    /// <summary>
    /// Check if ip is in the database
    /// </summary>
    public bool CheckIfIPExist(string ip)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM Auth WHERE IP = @ip";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@ip", ip);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.HasRows;
    }

    /// <summary>
    /// Get the token for a certain IP
    /// </summary>
    /// <param name="ip"></param>
    /// <returns></returns>
    public string GetTokenForkIp(string ip)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT Token FROM Auth WHERE IP = @ip";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@ip", ip);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return (string) reader["Token"];
        }
        return null;
    }

    /// <summary>
    /// Check if token or serverid exists in the database
    /// </summary>
    public bool CheckAuth(string token, ulong serverid = 0)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM Auth WHERE Serverid=@serverid OR Token=@token";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        cmd.Parameters.AddWithValue("@token", token);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.HasRows;
    }

    /// <summary>
    /// Check if onjoin is created num 0, num 1 to check if its enabled
    /// </summary>
    public bool CheckSevent(ulong serverid, int num = 0)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        var insert = num == 0 ? "SELECT * FROM OnJoin WHERE Serverid=@serverid" 
            : "SELECT * FROM OnJoin WHERE Serverid=@serverid AND sevent = 1";


        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();


        using var reader = cmd.ExecuteReader();
        return reader.HasRows;
    }

    /// <summary>
    /// Check if subscribed to notification
    /// </summary>
    public bool CheckIfNotifyExist(ulong serverid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM Notify WHERE Serverid=@serverid";


        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();


        using var reader = cmd.ExecuteReader();
        return reader.HasRows;
    }

    //TODO make separate methods instead of num
    /// <summary>
    /// This is for Server List events, num0 get channelid, num1 get message id
    /// </summary>
    public long GetSeventCH(ulong serverid, int num)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM OnJoin WHERE Serverid=@serverid";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            if (num == 0)
            {
                return (long) reader["Channelid"];
            }
            return (long) reader["Messageid"];
        }

        return 0;
    }

    /// <summary>
    /// This is for player events
    /// </summary>
    public long GetSubbedChannel(ulong serverid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM Notify WHERE Serverid=@serverid";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            return (long) reader["Channelid"];
        }

        return 0;
    }

    /// <summary>
    /// Check if channel or role is created already
    /// </summary>
    public bool CheckRoleAndChannel(ulong serverid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM OnJoin WHERE Serverid=@serverid";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.HasRows;
    }

    public bool CheckOnhold(string token, string ip = "Null")
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM Onhold WHERE Token=@token OR IP = @ip";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@token", token);
        cmd.Parameters.AddWithValue("@ip", ip);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.HasRows;
    }

    public void InsertAuth(ulong serverid, string token, string IP)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "INSERT INTO Auth(Serverid,Token,IP) VALUES (@serverid,@token,@ip)";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        cmd.Parameters.AddWithValue("@token", token);
        cmd.Parameters.AddWithValue("@ip", IP);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    public void InsertRole(ulong serverid, ulong roleid, ulong channelid, ulong messageid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert =
            "INSERT INTO OnJoin(Serverid,Roleid,Channelid,Messageid) VALUES (@serverid,@roleid,@channelid,@messageid)";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        cmd.Parameters.AddWithValue("@roleid", roleid);
        cmd.Parameters.AddWithValue("@channelid", channelid);
        cmd.Parameters.AddWithValue("@messageid", messageid);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    public void RemoveRole(ulong serverid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "DELETE FROM OnJoin WHERE Serverid = @serverid";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    public void LeaveServer(ulong serverid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = @"DELETE FROM Auth WHERE Serverid = @serverid;
                        DELETE FROM Notify WHERE Serverid = @serverid;
                        DELETE FROM OnJoin WHERE Serverid = @serverid;";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    public string GetTokenOfServer(ulong serverid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM Auth WHERE Serverid=@serverid";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? (string) reader["Token"] : null;
    }

    public long GetServerForToken(string token)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM Auth WHERE Token=@token";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@token", token);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? (long) reader["Serverid"] : 0;
    }

    /// <summary>
    /// Int 1 is for Onhold database, Int 2 is for Auth database
    /// </summary>
    public string GetIPForToken(string token, int db)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        var dbname = db == 1 ? "Onhold" : "Auth";

        string insert = $"SELECT * FROM {dbname} WHERE Token=@token";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@token", token);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        return reader.Read() ? (string) reader["IP"] : null;
    }

    //TODO why not split this into different methods?
    /// <summary>
    /// Int 0 returns Roleid , Int 1 Returns Channelid
    /// </summary>
    public long GetRoleandChannel(ulong serverId, int num = 0)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "SELECT * FROM OnJoin WHERE Serverid=@Serverid";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@Serverid", serverId);
        sqlconn.Open();
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            switch (num)
            {
                case 0:
                    return (long) reader["Roleid"];
                case 1:
                    return (long) reader["Channelid"];
            }
        }

        return 0;
    }

    public void InsertOnhold(string token, string ip)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);

        string insert;
        if (CheckOnhold("", ip))
        {
            insert = "UPDATE Onhold SET Token = @token WHERE IP = @ip";
        }
        else
        {
            insert = "INSERT INTO Onhold(Token,IP) VALUES (@token,@ip)";
        }
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@token", token);
        cmd.Parameters.AddWithValue("@ip", ip);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    //TODO is unused
    public void RemoveAuth(ulong serverid)
    {
        using var sqlconn = new SQLiteConnection(connectionstr);
        string insert = "DELETE FROM Auth WHERE Serverid = @serverid";
        var cmd = new SQLiteCommand(insert, sqlconn);
        cmd.Parameters.AddWithValue("@serverid", serverid);
        sqlconn.Open();
        cmd.ExecuteNonQuery();
    }

    private bool DbCheck(string fullpath)
    {
        return File.Exists(fullpath);
    }
}