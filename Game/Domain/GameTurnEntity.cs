using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;

namespace Game.Domain
{
    public class GameTurnEntity
    {
        [BsonElement]
        private Dictionary<string, PlayerDecision> decisions = new Dictionary<string, PlayerDecision>();

        public void AddPlayerDecision(Player player)
        {
            if (decisions.Count >= 2)
                throw new ArgumentOutOfRangeException();
            if (player.Decision == null)
                throw new ArgumentNullException();
            decisions[player.UserId.ToString()] = (PlayerDecision)player.Decision;
        }

        public Guid Id { get; set; }
        public int Index { get; set; }
        public Guid WinnerId { get; set; }
        public Guid GameId { get; set; }
        public PlayerDecision GetDecisionByUserId(Guid id) => decisions[id.ToString()];
    }
}