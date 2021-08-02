using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Models
{
   public class Auth
	{
		[Key]
		public int ID { get; set; }
        public ulong Serverid { get; set; }
        public string Token { get; set; }
        public string IP { get; set; }
    }
    public class Notify
    {
        [Key]
        public int ID { get; set; }
        public ulong Serverid { get; set; }
        public ulong Channelid { get; set; }
    }
    public class OnJoin
    {
        [Key]
        public int ID { get; set; }
        public ulong Serverid { get; set; }
        public ulong Channelid { get; set; }
        public ulong Roleid { get; set; }
        public int sevent { get; set; }
        public ulong Messageid { get; set; }
    }
    public class OnHold
    {
        [Key]
        public int ID { get; set; }
        public string Token { get; set; }
        public string IP { get; set; }
    }
}
