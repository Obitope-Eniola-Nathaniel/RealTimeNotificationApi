using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeNotificationApi.Hubs;
using RealTimeNotificationApi.Infrastructure;

namespace RealTimeNotificationApi.Controllers
{
    [Authorize] // require JWT
    [ApiController]
    [Route("api/[controller]")] // /api/tasks
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repository;
        private readonly INotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TasksController(
            ITaskRepository repository,
            INotificationRepository notificationRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _repository = repository;
            _notificationRepository = notificationRepository;
            _hubContext = hubContext;
        }

        // Helper to get current userId from JWT
        private string GetUserId() =>
            User.FindFirst("userId")?.Value ?? "unknown";

        // GET /api/tasks
        [HttpGet]
        public async Task<ActionResult<List<TaskItem>>> GetAll()
        {
            var tasks = await _repository.GetAllAsync();
            return Ok(tasks);
        }

        // GET /api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetById(string id)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task is null) return NotFound();
            return Ok(task);
        }

        // POST /api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create(TaskItem task)
        {
            if (string.IsNullOrEmpty(task.Id))
                task.Id = Guid.NewGuid().ToString();

            var created = await _repository.CreateAsync(task);

            var userId = GetUserId();
            var message = $"Task created: {created.Title}";

            // Store notification
            await _notificationRepository.CreateAsync(new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Message = message
            });

            // Broadcast in real-time (optional: to all or only to user’s group)
            await _hubContext.Clients.All
                .SendAsync("ReceiveMessage", message);

            return CreatedAtAction(nameof(GetById),
                new { id = created.Id }, created);
        }

        // PUT /api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, TaskItem task)
        {
            task.Id = id;

            var success = await _repository.UpdateAsync(id, task);
            if (!success) return NotFound();

            var userId = GetUserId();
            var message = $"Task updated: {task.Title}";

            await _notificationRepository.CreateAsync(new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Message = message
            });

            await _hubContext.Clients.All
                .SendAsync("ReceiveMessage", message);

            return NoContent();
        }

        // DELETE /api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var success = await _repository.DeleteAsync(id);
            if (!success) return NotFound();

            var userId = GetUserId();
            var message = $"Task deleted: {id}";

            await _notificationRepository.CreateAsync(new Notification
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                Message = message
            });

            await _hubContext.Clients.All
                .SendAsync("ReceiveMessage", message);

            return NoContent();
        }
    }
}
