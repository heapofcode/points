using System;
using System.Threading;
using System.Threading.Tasks;
using app.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace app.Services.JWT
{
    public class JwtClearExpToken : IHostedService, IDisposable
    {
        private Timer _timer;
        private readonly IServiceScopeFactory _scopeFactory;

        public JwtClearExpToken(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken stoppingToken)
        {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
            return Task.CompletedTask;
        }

        private async void DoWork(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                if (_context.Jwts.Any()) 
                {
                    var expiredTokens = await _context.Jwts.Where(x => x.ExpAtRef < DateTime.Now).ToListAsync();
                    if (expiredTokens != null)
                    {
                        _context.Jwts.RemoveRange(expiredTokens);
                        await _context.SaveChangesAsync();
                    }
                }
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            _timer?.Change(Timeout.Infinite, 0);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _timer?.Dispose();
        }
    }
}
