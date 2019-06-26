using Game.Domain;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;

namespace Tests
{
    [TestFixture]
    public class MongoTurnsRepositoryShould
    {
        [SetUp]
        public void SetUp()
        {
            var db = TestMongoDatabase.Create();
            db.DropCollection(MongoTurnsRepository.CollectionName);
            repo = new MongoTurnsRepository(db);

            g1 = new GameEntity(5);
            g1.GetType().GetProperty("CurrentTurnIndex").SetValue(g1, 3000);
            g1.GetType().GetProperty("Id").SetValue(g1, Guid.NewGuid());
            g2 = new GameEntity(5);
            g2.GetType().GetProperty("CurrentTurnIndex").SetValue(g2, 3000);
            g2.GetType().GetProperty("Id").SetValue(g2, Guid.NewGuid());
            g3 = new GameEntity(5);
            g3.GetType().GetProperty("CurrentTurnIndex").SetValue(g3, 3000);
            g3.GetType().GetProperty("Id").SetValue(g3, Guid.NewGuid());

            for (int i = 0; i < 3000; i++)
            {
                repo.Insert(new GameTurnEntity() { GameId = g1.Id, Index = i });
                repo.Insert(new GameTurnEntity() { GameId = g2.Id, Index = i });
                repo.Insert(new GameTurnEntity() { GameId = g3.Id, Index = i });
            }
        }

        private MongoTurnsRepository repo;
        private GameEntity g1;
        private GameEntity g2;
        private GameEntity g3;
        

        [Test(Description = "Тест на наличие индекса")]
        [MaxTime(15000)]
        public void SearchByLoginFast()
        {
            for (int i = 0; i < 3000; i++)
            {
                repo.GetFiveLastTurnEntity(g1);
                repo.GetFiveLastTurnEntity(g2);
                repo.GetFiveLastTurnEntity(g3);
            }
        }
    }
}
