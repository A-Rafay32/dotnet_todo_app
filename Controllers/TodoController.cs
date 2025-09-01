using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using TodoApp.Data;
using TodoApp.DTOs;
using TodoApp.Models;

namespace TodoApp.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoController : ControllerBase
{
    private readonly AppDbContext _db;

    public TodoController(AppDbContext db)
    {
        _db = db;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue("uid")!);

    [HttpGet]
    public IActionResult GetMyTodos()
    {
        var userId = GetUserId();
        var todos = _db.TodoItems.Where(t => t.UserId == userId).ToList();
        return Ok(todos);
    }

    [HttpGet("{id}")]
    public IActionResult GetTodoById(int id)
    {        
         var todo = _db.TodoItems
        .Include(t => t.User)              
        .FirstOrDefault(t => t.Id == id);

        if (todo == null)
            return NotFound();

            return Ok(todo);
    }


    [HttpPost]
    [Authorize] 
    public IActionResult Create([FromBody] TodoDto item)
    {
        var userId = GetUserId();
        var user = _db.Users.FirstOrDefault(u => u.Id == userId);
        if (user == null) return Unauthorized();

        var todo = new TodoItem
        {
            Title = item.Title,        
            UserId = userId           
        };

        _db.TodoItems.Add(todo);
        _db.SaveChanges();

        return Ok(new
        {
            todo.Id,
            todo.Title,
            todo.UserId
        });
    }


    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] TodoItem updated)
    {
        var userId = GetUserId();
        var todo = _db.TodoItems.FirstOrDefault(t => t.Id == id && t.UserId == userId);
        if (todo == null) return NotFound();

        todo.Title = updated.Title;
        todo.IsCompleted = updated.IsCompleted;
        _db.SaveChanges();
        return Ok(todo);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        var userId = GetUserId();
        var todo = _db.TodoItems.FirstOrDefault(t => t.Id == id && t.UserId == userId);
        if (todo == null) return NotFound();

        _db.TodoItems.Remove(todo);
        _db.SaveChanges();
        return NoContent();
    }
}
