using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace SpaceGameDemo {

    public class ServerItem {
        public string title;
        public int players;
        public int maxPlayers;
        public string gameType;
        public string map;
        public int ping;
        public bool isFavorite;
    }

    [Template("SpaceGameDemo/JoinServer/JoinServer.xml")]
    public class JoinServer : UIElement {

        public List<ServerItem> servers;

        public override void OnCreate() {
            servers = new List<ServerItem>() {
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "# FREE COOKIES! FOR EVERYONE",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[TDM] 24/7 TDM! JOIN NOW!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "# FREE COOKIES! FOR EVERYONE",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[TDM] 24/7 TDM! JOIN NOW!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "# FREE COOKIES! FOR EVERYONE",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[TDM] 24/7 TDM! JOIN NOW!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "# FREE COOKIES! FOR EVERYONE",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[TDM] 24/7 TDM! JOIN NOW!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "# FREE COOKIES! FOR EVERYONE",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[TDM] 24/7 TDM! JOIN NOW!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "# FREE COOKIES! FOR EVERYONE",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[TDM] 24/7 TDM! JOIN NOW!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "# FREE COOKIES! FOR EVERYONE",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[TDM] 24/7 TDM! JOIN NOW!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "# FREE COOKIES! FOR EVERYONE",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[TDM] 24/7 TDM! JOIN NOW!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "# FREE COOKIES! FOR EVERYONE",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[TDM] 24/7 TDM! JOIN NOW!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
                new ServerItem() {
                    title = "[EPIC] EPIC GAME!",
                    players = 16,
                    maxPlayers = 22,
                    gameType = "TDM",
                    map = "Deep Space",
                    ping = 10,
                    isFavorite = false
                },
            };
        }

        public override void OnUpdate() {
            switch ((int) (60 * Random.value)) {
                case 20: servers[(int) (servers.Count * Random.value)].ping = (int) (120 * Random.value);
                    break;
                case 50: servers[(int) (servers.Count * Random.value)].players = (int) (22 * Random.value);
                    break;
            }
        }
    }

}