using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using RealTimeNotificationApi.Hubs;
using RealTimeNotificationApi.Infrastructure;

namespace RealTimeNotificationApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public TasksController(
            ITaskRepository repository,
            IHubContext<NotificationHub> hubContext)
        {
            _repository = repository;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<ActionResult<List<TaskItem>>> GetAll()
        {
            var tasks = await _repository.GetAllAsync();
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetById(string id)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task is null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> Create(TaskItem task)
        {
            // For demo, ensure ID is set (Mongo can also generate)
            if (string.IsNullOrEmpty(task.Id))
            {
                task.Id = Guid.NewGuid().ToString();
            }

            var created = await _repository.CreateAsync(task);

            await _hubContext.Clients.All
                .SendAsync("ReceiveMessage", $"Task created: {created.Title}");

            return CreatedAtAction(nameof(GetById),
                new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, TaskItem task)
        {
            task.Id = id;

            var success = await _repository.UpdateAsync(id, task);
            if (!success) return NotFound();

            await _hubContext.Clients.All
                .SendAsync("ReceiveMessage", $"Task updated: {task.Title}");

            return NoContent();
        }

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
