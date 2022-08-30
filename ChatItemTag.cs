using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace ChatItemTag
{
    [ApiVersion(2, 1)]
    public class ChatItemTag : TerrariaPlugin
    {
        public override string Author => "RidnRaven";
        public override string Description => "Simplify item tagging in chat for non keyboard & mouse user";
        public override string Name => "Chat Item Tagging";
        public override Version Version => new Version(1, 0, 0, 0);

        public ChatItemTag(Main game) : base(game)
        {
        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerChat.Register(this, OnChat);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) ServerApi.Hooks.ServerChat.Deregister(this, OnChat);

            base.Dispose(disposing);
        }

        private void OnChat(ServerChatEventArgs args)
        {
            var ply = TShock.Players[args.Who];
            var plyChat = args.Text;

            if (ply == null) return;

            const string pattern = @"\[.*?\]";
            var match = Regex.Match(plyChat, pattern);
            var listResult = new List<MatchResult>();
            while (match.Success)
            {
                var replaced = "";
                var value = match.Value.Replace("[", "").Replace("]", "");
                var itemList = TShock.Utils.GetItemByName(value);
                if (itemList.Count == 0 || itemList.Count > 1)
                {
                    replaced = match.Value;
                }
                else
                {
                    replaced = $"[i/s1:{itemList.First().netID}]";
                }
                
                listResult.Add(new MatchResult(match.Value, match.Length, replaced));
                match = match.NextMatch();
            }

            plyChat = listResult.Aggregate(plyChat, (current, o) => current.Replace(o.Original, o.Replacement));
            var resultText = string.Format(TShock.Config.Settings.ChatFormat, ply.Group.Name, ply.Group.Prefix,
                ply.Name, ply.Group.Suffix, plyChat);
            TShock.Utils.Broadcast(resultText, new Color(ply.Group.R, ply.Group.G, ply.Group.B));
            args.Handled = true;
        }
    }

    public class MatchResult
    {
        public string Original { get; set; }
        public int Length { get; set; }
        public string Replacement { get; set; }

        public MatchResult(string original, int length, string replacement)
        {
            Original = original;
            Length = length;
            Replacement = replacement;
        }
    }
}