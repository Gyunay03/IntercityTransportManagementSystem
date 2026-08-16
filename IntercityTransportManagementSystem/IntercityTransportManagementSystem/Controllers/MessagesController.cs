using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IntercityTransportManagementSystem.Models;
using IntercityTransportManagementSystem.Services;
using Microsoft.AspNetCore.SignalR;
using IntercityTransportManagementSystem.Enums;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using IntercityTransportManagementSystem.Hubs;

[Authorize]
public class MessagesController : Controller
{
    private readonly IntercityTransportManagementSystemDatabaseContext _context;
    private readonly INotificationService _notificationService;
    private readonly IHubContext<MessageHub> _hubContext;

    public MessagesController(IntercityTransportManagementSystemDatabaseContext context, INotificationService notificationService, IHubContext<MessageHub> hubContext)
    {
        _context = context;
        _notificationService = notificationService;
        _hubContext = hubContext;
    }

    // GET: Messages
    public async Task<IActionResult> Index(string searchString, string sortOrder, MessageType? typeFilter, bool? isResolvedFilter, int page = 1, int pageSize = 10)    
    {
        var userId = GetCurrentUserId();
        
        if (userId == null)
        {
            return Unauthorized();
        }

        var messagesQuery = _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Schedule)
                .ThenInclude(s => s.Route)
            .AsNoTracking()
            .AsQueryable();

        // Ако потребителят не е администратор, вижда само съобщенията, в които участва
        if (!User.IsInRole("Administrator"))
        {
            messagesQuery = messagesQuery.Where(m =>
                m.SenderId == userId.Value ||
                m.ReceiverId == userId.Value ||
                (m.MessageType == MessageType.ProblemReport && m.ReceiverId == null));
        }

        // Филтриране по тип (StandardMessage/ProblemReport)
        if (typeFilter.HasValue)
        {
            messagesQuery = messagesQuery.Where(m => m.MessageType == typeFilter.Value);
        }
        
        // Филтриране по статус (Решен/Нерешен проблем)
        if (isResolvedFilter.HasValue)
        {
            messagesQuery = messagesQuery.Where(m => m.IsResolved == isResolvedFilter.Value);
        }

        // Търсене по име на изпращач или по съдържание
        if (!string.IsNullOrWhiteSpace(searchString))
        {
            messagesQuery = messagesQuery.Where(m =>
                m.Sender.Name.Contains(searchString) ||
                m.Sender.LastName.Contains(searchString) ||
                m.Sender.Email.Contains(searchString) ||
                m.Content.Contains(searchString));
        }

        // Сортиране
        switch (sortOrder)
        {
            case "sender":
                messagesQuery = messagesQuery.OrderBy(m =>
                (m.Sender.Name + " " + m.Sender.LastName));
                break;
            
            case "sender_descending":
                messagesQuery = messagesQuery.OrderByDescending(m =>
                (m.Sender.Name + " " + m.Sender.LastName));
                break;
            
            case "receiver":
                messagesQuery = messagesQuery.OrderBy(m =>
                (m.Receiver.Name + " " + m.Receiver.LastName));
                break;
            case "receiver_descending":
                messagesQuery = messagesQuery.OrderByDescending(m =>
                (m.Receiver.Name + " " + m.Receiver.LastName));
                break;
            default:
                messagesQuery = messagesQuery.OrderBy(m => m.SentAt);
                break;
        }

        // Странициране
        var totalCount = await messagesQuery.CountAsync();
        var messages = await messagesQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        ViewBag.SearchString = searchString;
        ViewBag.SortOrder = sortOrder;
        ViewBag.TypeFilter = typeFilter;
        ViewBag.IsResolvedFilter = isResolvedFilter;

        return View(messages);
    }

    // GET: Messages/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var message = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Receiver)
            .Include(m => m.Schedule)
                .ThenInclude(s => s.Route)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (message == null)
        {
            return NotFound();
        }

        // Проверка за достъп
        bool isAuthorized = User.IsInRole("Administrator") ||
                            message.SenderId == userId.Value ||
                            message.ReceiverId == userId.Value ||
                            (message.MessageType == MessageType.ProblemReport && message.ReceiverId == null);

        if (!isAuthorized)
        {
            return Forbid();
        }

        // Зареждане на цялата история на комуникацията на същото разписание
        var conversation = await _context.Messages
            .Include(m => m.Sender)
            .Where(m => m.ScheduleId == message.ScheduleId && m.MessageType == message.MessageType)
            .OrderBy(m => m.SentAt)
            .ToListAsync();

        ViewBag.Conversation = conversation;

        return View(message);
    }

    // GET: Messages/Create
    [HttpGet]
    public async Task <IActionResult> Create(int? scheduleId, MessageType? type)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        // Избиране на първия достъпен курс, ако не е подаден валиден ScheduleId
        int selectedScheduleId = scheduleId ?? 0;
        if (selectedScheduleId == 0)
        {
            var allowedScheduleIds = await GetUserAllowedScheduleIdsAsync(userId.Value);
            selectedScheduleId = allowedScheduleIds.FirstOrDefault();
        }

        await PopulateSchedulesDropdown(userId.Value, selectedScheduleId);

        // Ако потребителят е администратор и има избран курс, се зареждат пътниците за този курс
        if (User.IsInRole("Administrator") && selectedScheduleId > 0)
        {
            await PopulatePassengersDropdown(selectedScheduleId);
        }

        var model = new Message
        {
            ScheduleId = selectedScheduleId,
            MessageType = type ?? MessageType.StandardMessage
        };

        return View(model);
    }

    // POST: Messages/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Message message)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
        {
            return Unauthorized();
        }

        // Валидация за достъп до курса (ScheduleId)
        bool isScheduleAllowed = await CanUserAccessScheduleAsync(userId.Value, message.ScheduleId);
        if (!isScheduleAllowed)
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            message.SenderId = userId.Value;
            message.SentAt = DateTime.Now;
            message.IsResolved = false;

            // Автоматично определяне на получателя (ReceiverId)
            if (!message.ReceiverId.HasValue)
            {
                var schedule = await _context.BusSchedules
                    .Include(s => s.Driver)
                    .FirstOrDefaultAsync(s => s.Id == message.ScheduleId);

                if (schedule != null)
                {
                    if (User.IsInRole("Administrator"))
                    {
                        if (!message.ReceiverId.HasValue || message.ReceiverId.Value <= 0)
                        {
                            message.ReceiverId = schedule.Driver?.UserId;
                        }
                    }

                    else if (User.IsInRole("Passenger"))
                    {
                        if (message.MessageType == MessageType.ProblemReport)
                        {
                            message.ReceiverId = null;
                        }
                        else if (schedule?.Driver != null)
                        {
                            message.ReceiverId = schedule.Driver.UserId;
                        }
                    }
                    else if (User.IsInRole("Driver"))
                    {
                        // Ако шофьорът изпраща сигнал -> получател е администраторът
                        if (message.MessageType == MessageType.ProblemReport)
                        {
                            message.ReceiverId = null;
                        }
                    }
                }
            }
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Изпращане на системно известие
            if (message.ReceiverId.HasValue)
            {
                // Известие за конкретен шофьор/пътник
                await _notificationService.CreateNotificationAsync(
                message.ReceiverId.Value,
                message.MessageType == MessageType.ProblemReport ? $"Получихте нов сигнал относно курс #{message.ScheduleId}." : $"Ново съобщение за курс #{message.ScheduleId}",
                message.Content, NotificationType.SystemMessage);
            }
            else 
            {
                // Когато ReceiverId е null, се подава сигнал към администратора (получава известие)
                var adminUserIds = await _context.Users
                    .Where(u => u.Role == UserRole.Administrator)
                    .Select(u => u.Id)
                    .ToListAsync();

                foreach (var adminId in adminUserIds)
                {
                    await _notificationService.CreateNotificationAsync(
                        adminId, $"Нов сигнал към администратора за курс #{message.ScheduleId}",
                        message.Content, NotificationType.SystemMessage);
                }
            }

            // Изпращане на живо чрез SignalR към курса
            await _hubContext.Clients.Group($"Schedule_{message.ScheduleId}")
                .SendAsync("ReceiveMessage", userId.Value, message.Content, message.SentAt.ToString("g"), (int)message.MessageType);

            TempData["Success"] = message.MessageType == MessageType.ProblemReport
                ? "Сигналът за проблем e изпратен успешно!"
                : "Съобщението e изпратено!";

            return RedirectToAction(nameof(Index));
        }

        await PopulateSchedulesDropdown(userId.Value, message.ScheduleId);
        
        if (User.IsInRole("Administrator"))
        {
            await PopulatePassengersDropdown(message.ScheduleId, message.ReceiverId);
        }

        return View(message);
    }

    // Метод за маркиране на проблем като решен/активен
    // POST: Messages/ToggleResolvedStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleResolvedStatus(int id)
    {
        var userId = GetCurrentUserId();
        
        if (userId == null)
        { 
            return Unauthorized(); 
        }

        var message = await _context.Messages.FindAsync(id);

        if (message == null)
        {
            return NotFound();
        }

        // Само администратор или шофьор може да маркира проблем като решен
        if (!User.IsInRole("Administrator") && !User.IsInRole("Driver"))
        {
            return Forbid();
        }

        message.IsResolved = !message.IsResolved;
        await _context.SaveChangesAsync();

        // Известие до потребителя, който е изпратил сигнала
        string statusText = message.IsResolved ? "решен" : "отново активен";
        await _notificationService.CreateNotificationAsync(
            message.SenderId, "Обновен статус на сигнал",
            $"Вашият сигнал относно курс #{message.ScheduleId} беше маркиран като {statusText}.",
            NotificationType.SystemMessage);

        TempData["Info"] = $"Статусът на сигнала беше промене на: {(message.IsResolved ? "Решен" : "Активен")}.";

        return RedirectToAction(nameof(Details), new { id });
    }

    // GET: Messages/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        
        if (userId == null)
        {
            return Unauthorized();
        }

        var message = await _context.Messages.FindAsync(id);
        
        if (message == null)
        {
            return NotFound();
        }

        // Само авторът може да редактира своето съобщение
        if (message.SenderId != userId.Value)
        {
            return Forbid();
        }

        return View(message);
    }

    // POST: Messages/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? id, string content)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var message = await _context.Messages.FindAsync(id);

        if (message == null)
        {
            return NotFound();
        }

        if (message.SenderId != userId.Value)
        {
            return Forbid();
        }

        if (!string.IsNullOrWhiteSpace(content))
        {
            message.Content = content;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Съобщението е редактирано успешно.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.AddModelError("Content", "Съдържанието на съобщението не може да бъде празно.");
        return View(message);
    }

    // GET: Messages/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized();
        }

        var message = await _context.Messages
            .Include(m => m.Sender)
            .Include(m => m.Schedule)
                .ThenInclude(s => s.Route)
            .FirstOrDefaultAsync(m => m.Id == id);
        
        if (message == null)
        {
            return NotFound();
        }

        // Само администраторът или изпращащият могат да изтрият съобщението
        bool canDelete = User.IsInRole("Administrator") || message.SenderId == userId.Value;
        if (!canDelete)
        {
            return Forbid();
        }
        return View(message);
    }

    // POST: Messages/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var userId = GetCurrentUserId();
        
        if (userId == null)
        {
            return Unauthorized();
        }
        
        var message = await _context.Messages.FindAsync(id);
        if (message != null)
        {
            bool canDelete = User.IsInRole("Administrator") || message.SenderId == userId.Value;
            if (!canDelete)
            {
                return Forbid();
            }

            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            TempData["Info"] = "Съобщението/сигналът е изтрит(о).";
        }

        return RedirectToAction(nameof(Index));
    }

    private int? GetCurrentUserId()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userIdString, out int userId) ? userId : null;
    }

    private async Task<List<int>> GetUserAllowedScheduleIdsAsync(int userId)
    {
        if (User.IsInRole("Administrator"))
        {
            return await _context.BusSchedules.Select(s => s.Id).ToListAsync();
        }
        
        if (User.IsInRole("Passenger"))
        {
            return await _context.Reservations
                .Include(r => r.Passenger)
                .Where(r => r.Passenger.UserId == userId)
                .Select(r => r.ScheduleId)
                .Distinct()
                .ToListAsync();
        }
        
        if (User.IsInRole("Driver"))
        {
            var driver = await _context.Drivers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.UserId == userId);

            if (driver == null)
            {
                return new List<int>();
            }

            return await _context.BusSchedules
                .Where(s => s.Driver != null && s.Driver.UserId == userId)
                .Select(s => s.Id)
                .ToListAsync();
        }

        return new List<int>();
    }

    private async Task<bool> CanUserAccessScheduleAsync(int userId, int scheduleId)
    {
        var allowedScheduleIds = await GetUserAllowedScheduleIdsAsync(userId);
        return allowedScheduleIds.Contains(scheduleId);
    }

    // Помощен метод за зареждане на разписанията
    private async Task PopulateSchedulesDropdown(int userId, int? selectedScheduleId)
    {
        var allowedScheduleIds = await GetUserAllowedScheduleIdsAsync(userId);

        var schedulesQuery = _context.BusSchedules
            .Include(s => s.Route)
            .Include(s => s.Driver)
            .Where(s => User.IsInRole("Administrator") || allowedScheduleIds.Contains(s.Id))
            .AsNoTracking();

        // Ако е пътник, се показват само неговите резервирани курсове
        if (User.IsInRole("Passenger"))
        {
            var userScheduleIds = await _context.Reservations
                .Include(r => r.Passenger)
                .Where(r => r.Passenger.UserId == userId)
                .Select(r => r.ScheduleId)
                .Distinct()
                .ToListAsync();

            schedulesQuery = schedulesQuery.Where(s => userScheduleIds.Contains(s.Id));
        }
        
        // Ако е шофьор, се показват само неговите резервирани курсове
        if (User.IsInRole("Driver"))
        {
            schedulesQuery = schedulesQuery.Where(s => s.Driver.UserId == userId);
        }

        var schedulesList = await schedulesQuery
            .Select(s => new
            {
                s.Id,
                Info = $"{s.Route.StartDestination} - {s.Route.FinalDestination} ({s.TravelDate:dd.MM.yyyy} {s.DepartureTime})"
            })
            .ToListAsync();

        ViewData["ScheduleId"] = new SelectList(schedulesList, "Id", "Info", selectedScheduleId);
    }

    // Помощен метод за зареждане на пътниците
    private async Task PopulatePassengersDropdown(int scheduleId, int? selectedPassengerId = null)
    {
        var passengers = await _context.Reservations
            .Where(r => r.ScheduleId == scheduleId && r.Passenger != null)
            .Select(r => new
            {
                UserId = r.Passenger.UserId,
                FullName = $"{r.Passenger.Name} {r.Passenger.LastName} ({r.Passenger.Email})"
            })
            .Distinct()
            .ToListAsync();

        ViewBag.Passengers = new SelectList(passengers, "UserId", "FullName", selectedPassengerId);
    }
}
