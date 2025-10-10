using CloudGames.Users.Domain.Entities;
using CloudGames.Users.Domain.Repositories;
using CloudGames.Users.Infra.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CloudGames.Users.Infra.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UsersDbContext _db;
    public UserRepository(UsersDbContext db) { _db = db; }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
        => await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);

    public async Task<List<User>> GetAllAsync(CancellationToken cancellationToken)
        => await _db.Users.ToListAsync(cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
        => await _db.Users.AddAsync(user, cancellationToken);

    public void Update(User user)
        => _db.Users.Update(user);
}

