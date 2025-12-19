using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeNotificationApi.Hubs;
using RealTimeNotificationApi.Infrastructure;

namespace RealTimeNotificationApi.Controllers
{
    // Marks this as a Web API controller and sets base route: /api/tasks
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        // Repository for MongoDB operations
        private readonly ITaskRepository _repository;
        // SignalR hub context to broadcast messages
        private readonly IHubContext<NotificationHub> _hubContext;

        // Dependencies are injected by ASP.NET Core
        public TasksController(
            ITaskRepository repository,
            IHubContext<NotificationHub> hubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
        }

        // GET /api/tasks
        // Returns all tasks from MongoDB
        [HttpGet]
        public async Task<ActionResult<List<TaskItem>>> GetAll()
        {
            var tasks = await _repository.GetAllAsync();
            return Ok(tasks);
        }

        // GET /api/tasks/{id}
        // Returns a single task by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetById(string id)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task is null) return NotFound();
            return Ok(task);
        }

        // POST /api/tasks
        // Creates a new task and broadcasts a "created" notification
        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create(TaskItem task)
        {
            // If client didn't send Id, generate a GUID
            if (string.IsNullOrEmpty(task.Id))
                task.Id = Guid.NewGuid().ToString();

            var created = await _repository.CreateAsync(task);

            // Notify all connected SignalR clients
            await _hubContext.Clients.All
                .SendAsync("ReceiveMessage", $"Task created: {created.Title}");

            // Return 201 Created with location header pointing to GetById
            return CreatedAtAction(nameof(GetById),
                new { id = created.Id }, created);
        }

        // PUT /api/tasks/{id}
        // Updates an existing task and sends an "updated" notification
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, TaskItem task)
        {
            task.Id = id;

            var success = await _repository.UpdateAsync(id, task);
            if (!success) return NotFound();

            await _hubContext.Clients.All
                .SendAsync("ReceiveMessage", $"Task updated: {task.Title}");

            return NoContent(); // 204
        }

        // DELETE /api/tasks/{id}
        // Deletes a task and sends a "deleted" notification
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _repository.DeleteAsync(id);
            if (!success) return NotFound();

            await _hubContext.Clients.All
                .SendAsync("ReceiveMessage", $"Task deleted: {id}");

            return NoContent();
        }
    }
}
