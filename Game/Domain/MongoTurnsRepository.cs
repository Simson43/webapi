using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;

namespace Game.Domain
{
    public class MongoTurnsRepository : ITurnsRepository
    {
        private readonly IMongoCollection<GameTurnEntity> turnCollection;
        public const string CollectionName = "gameTurns";

        public MongoTurnsRepository(IMongoDatabase db)
        {
            turnCollection = db.GetCollection<GameTurnEntity>(CollectionName);
            turnCollection.Indexes.CreateOne("{Index : 1, GameId : 2}", new CreateIndexOptions() { Unique = true });
        }

        public GameTurnEntity Insert(GameTurnEntity gameTurn)
        {
            turnCollection.InsertOne(gameTurn);
            return gameTurn;
        }

        public GameTurnEntity FindBiId(Guid id)
        {
            return turnCollection.Find(x => x.Id == id).SingleOrDefault();
        }

        public void Update(GameTurnEntity gameTurn)
        {
            turnCollection.ReplaceOne(x => x.Id == gameTurn.Id, gameTurn);
        }

        public List<GameTurnEntity> GetFiveLastTurnEntity(GameEntity game)
        {
            return turnCollection.Find(x => x.GameId == game.Id && x.Index > game.CurrentTurnIndex - 5).SortBy(x => x.Index).ToList();
        }
    }
}