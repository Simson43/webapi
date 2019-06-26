using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Game.Domain;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApi.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    public class UsersController : Controller
    {
        private IUserRepository userRepository;
        // Чтобы ASP.NET положил что-то в userRepository требуется конфигурация
        public UsersController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        [HttpHead("{userId}")]
        [HttpGet("{userId}", Name = nameof(GetUserById))]
        public ActionResult<UserDto> GetUserById([FromRoute] Guid userId)
        {
            var userEntity = userRepository.FindById(userId);
            return userEntity != null ?
                Ok(Mapper.Map<UserDto>(userEntity)) :
                NotFound() as ActionResult;
        }

        [HttpPost]
        public IActionResult CreateUser([FromBody] CreatedUserDto user)
        {
            if (user == null)
                return BadRequest();
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            var createdUser = Mapper.Map<UserEntity>(user);
            createdUser = userRepository.Insert(createdUser);
            return CreatedAtRoute(nameof(GetUserById), new { userId = createdUser.Id }, createdUser.Id);
        }

        [HttpPut("{userId}")]
        public IActionResult UpdateUser([FromRoute] Guid userId, [FromBody] UpdatedUserDto user)
        {
            if (user == null || userId == Guid.Empty)
                return BadRequest();
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            user.Id = userId;
            userRepository.UpdateOrInsert(Mapper.Map<UserEntity>(user), out bool isInserted);
            return isInserted ?
                CreatedAtRoute(nameof(GetUserById), new { userId = user.Id }, userId) :
                NoContent() as ActionResult;
        }

        [HttpPatch("{userId}")]
        public IActionResult PartiallyUpdateUser([FromRoute] Guid userId, [FromBody] JsonPatchDocument<UpdatedUserDto> pathcDoc)
        {
            if (userId == Guid.Empty)
                return NotFound();
            if (pathcDoc == null || !TryValidateModel(pathcDoc))
                return BadRequest();
            var user = userRepository.FindById(userId);
            if (user == null)
                return NotFound();
            var updateUser = new UpdatedUserDto() { Id = userId };
            pathcDoc.ApplyTo(updateUser, ModelState);
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            Mapper.Map(updateUser, user);
            userRepository.Update(user);
            return NoContent() as ActionResult;
        }

        [HttpDelete("{userId}")]
        public IActionResult DeleteUser([FromRoute] Guid userId)
        {
            if (userId == Guid.Empty)
                return NotFound();
            var user = userRepository.FindById(userId);
            if (user == null)
                return NotFound();
            userRepository.Delete(userId);
            return NoContent();
        }

        [HttpGet]
        public IActionResult GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            if (pageNumber <= 0)
                pageNumber = 1;
            if (pageSize <= 0)
                pageSize = 1;
            if (pageSize > 20)
                pageSize = 20;
            var users = userRepository.GetPage(pageNumber, pageSize);
            var us = Mapper.Map<IEnumerable<UserDto>>(users);
            var linkGenerator = HttpContext.RequestServices.GetRequiredService<LinkGenerator>();
            var paginationHeader = new
            {
                previousPageLink = linkGenerator.GetUriByRouteValues(HttpContext,
                    "Имя метода из атрибута", new { pageNumber = pageNumber - 1, pageSize }),
                nextPageLink = linkGenerator.GetUriByRouteValues(HttpContext,
                    "Имя метода из атрибута", new { pageNumber = pageNumber + 1, pageSize }),
                totalCount = users.TotalCount,
                pageSize,
                currentPage = pageNumber,
                totalPages = users.TotalPages
            };
            Response.Headers.Add("X-Pagination", JsonConvert.SerializeObject(paginationHeader));
            return Ok(us);
        }

        [HttpOptions]
        public IActionResult GetAllowMethods()
        {
            Response.Headers.Add("Allow", "POST,GET,OPTIONS");
            return Ok();
        }
    }
}