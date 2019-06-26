using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class GamesController : Controller
    {
        private IGameRepository gameRepo;
        private ITurnsRepository turnRepo;
        private IUserRepository userRepo;

        public GamesController(IGameRepository gameRepo, ITurnsRepository turnRepo, IUserRepository userRepo)
        {
            this.gameRepo = gameRepo;
            this.turnRepo = turnRepo;
            this.userRepo = userRepo;
        }

        [HttpGet("{gameId}", Name = nameof(GetGameById))]
        public ActionResult<GameDto> GetGameById([FromRoute]Guid gameId)
        {
            if (gameId == Guid.Empty)
                return BadRequest();
            var game = gameRepo.FindById(gameId);
            return game != null ?
                Ok(Mapper.Map<GameDto>(game)) :
                NotFound() as ActionResult;
        }

        [HttpPost("{userId}")]
        public IActionResult CreateGame([FromRoute]Guid userId, [FromQuery] int turnsCount = 5)
        {
            if (userId == Guid.Empty)
                return BadRequest();
            var user = userRepo.FindById(userId);
            if (user == null)
                return NotFound();
            var game = new GameEntity(turnsCount);
            game.AddPlayer(user);
            gameRepo.Insert(game);
            user.CurrentGameId = game.Id;
            userRepo.Update(user);
            return CreatedAtRoute(nameof(GetGameById), new { gameId = game.Id }, game.Id);
        }

        [HttpPut]
        public IActionResult JoinToGame([FromQuery]Guid gameId, [FromQuery]Guid userId)
        {
            if (gameId == Guid.Empty || userId == Guid.Empty)
                return BadRequest();
            var user = userRepo.FindById(userId);
            var game = gameRepo.FindById(gameId);
            if (user == null || game == null)
                return NotFound();
            if (game.Status != GameStatus.WaitingToStart)
                return BadRequest();
            game.AddPlayer(user);
            if (gameRepo.TryUpdateWaitingToStart(game))
            {
                user.CurrentGameId = game.Id;
                userRepo.Update(user);
                return NoContent();
            }
            return StatusCode(500);
        }

        [HttpPatch("start/{gameId}")]
        public IActionResult StartGame([FromRoute] Guid gameId)
        {
            if (gameId == Guid.Empty)
                return BadRequest();
            var game = gameRepo.FindById(gameId);
            if (game == null)
                return NotFound();
            return game.Status == GameStatus.Playing && game.Players.Count == 2 ?
                NoContent() :
                BadRequest() as IActionResult;
        }

        [HttpPatch("{gameId}")]
        public IActionResult DoStep([FromRoute]Guid gameId, [FromQuery]Guid userId, [FromQuery] int decision)
        {
            if (gameId == Guid.Empty || userId == Guid.Empty || decision < 1 || decision > 3)
                return BadRequest();
            var game = gameRepo.FindById(gameId);
            if (game == null)
                return NotFound();
            if (game.Status != GameStatus.Playing)
                return BadRequest();
            game.SetPlayerDecision(userId, (PlayerDecision)decision);

            GameTurnEntity turn = null;
            if (game.HaveDecisionOfEveryPlayer)
            {
                turn = game.FinishTurn();
                turnRepo.Insert(turn);
            }
            gameRepo.Update(game);
            return turn != null
                ? Ok(GetScore(game, turn))
                : NoContent() as IActionResult;
        }

        private static dynamic GetScore(GameEntity game, GameTurnEntity turn)
        {
            dynamic obj;
            if (!game.IsFinished())
            {
                obj = new
                {
                    isFinish = false,
                    decisions = new Dictionary<Guid, PlayerDecision>()
                    {
                        { game.Players[0].UserId, turn.GetDecisionByUserId(game.Players[0].UserId) },
                        { game.Players[1].UserId, turn.GetDecisionByUserId(game.Players[1].UserId) }
                    },
                    winnerId = turn.WinnerId
                };
            }
            else
            {
                obj = new
                {
                    isFinish = true,
                    score = new Dictionary<Guid, int>()
                    {
                        { game.Players[0].UserId, game.Players[0].Score },
                        { game.Players[1].UserId, game.Players[1].Score }
                    },
                    winnerId = game.Players[0].Score > game.Players[1].Score
                    ? game.Players[0].UserId
                    : game.Players[1].UserId
                };
            }

            return obj;
        }

        [HttpPatch("end/{userId}")]
        public IActionResult EndGame([FromRoute]Guid userId)
        {
            if (userId == Guid.Empty)
                return BadRequest();
            var user = userRepo.FindById(userId);
            if (user == null)
                return NotFound();
            if (user.CurrentGameId == null)
            {
                user.CurrentGameId = null;
                userRepo.Update(user);
            }
            return Ok();
        }
    }
}