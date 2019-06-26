using System;
using System.Collections.Generic;

namespace Game.Domain
{
    public interface ITurnsRepository
    {
        GameTurnEntity Insert(GameTurnEntity gameTurn);
        GameTurnEntity FindBiId(Guid id);
        void Update(GameTurnEntity gameTurn);
        List<GameTurnEntity> GetFiveLastTurnEntity(GameEntity game);
    }
}