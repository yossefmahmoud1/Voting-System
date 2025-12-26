
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using SurveyBasket.Abstraction.Consts;
using SurveyBasket.Helpers;
using SurveyBasket.Repositeryes.Interfaces;
using static Org.BouncyCastle.Crypto.Engines.SM2Engine;

namespace SurveyBasket.Services.Implementation
{
    public  class NotifcationService(ApplicationDbContext context, UserManager<Application_User> userManager ,IHttpContextAccessor httpContextAccessor, IEmailSender emailSender) : INotifcationService
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<Application_User> userManager = userManager;
        private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
        private readonly IEmailSender emailSender = emailSender;

        public async Task SendNewPollNotficationsAsync(int? pollId = null)
        {
            IEnumerable<Poll> polls = [];

            if (pollId.HasValue)
            {
                var poll = await context.Polls
                    .Where(p => p.IsPublished && p.Id == pollId.Value)
                    .FirstOrDefaultAsync();

                if (poll == null)
                    return;

                polls = new[] { poll };
            }
            else
            {
                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                polls = await context.Polls
                    .Where(p => p.IsPublished && p.StartsAt == today)
                    .AsNoTracking()
                    .ToListAsync();
            }

            var users = await userManager.GetUsersInRoleAsync(DefaultRoles.Member);
            var Origin = httpContextAccessor.HttpContext?.Request.Headers.Origin;

            foreach (var poll in polls)
            {
                foreach (var user in users)
                {
                    var placeholders = new Dictionary<string, string>
            {
                { "{{name}}", user.FristName ?? "User" },
                { "{{pollTill}}", poll.Title },
                { "{{endDate}}", poll.EndsAt.ToString("d") },
                { "{{url}}", $"{Origin}/Polls/Start?id={poll.Id}" }
            };

                    var body = EmailBodyBuilder.GenrateEmailBody("PollNotification", placeholders);

                    await emailSender.SendEmailAsync(
                        user.Email!,
                        $"New Poll Available! {poll.Title}",
                        body
                    );
                }
            }
        }
    }
}
