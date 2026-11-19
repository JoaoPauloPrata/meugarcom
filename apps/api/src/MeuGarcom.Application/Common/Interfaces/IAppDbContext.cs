using MeuGarcom.Domain.Accounts;
using MeuGarcom.Domain.Ai;
using MeuGarcom.Domain.Conversations;
using MeuGarcom.Domain.Menus;
using Microsoft.EntityFrameworkCore;

namespace MeuGarcom.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Account> Accounts { get; }
    DbSet<Restaurant> Restaurants { get; }
    DbSet<Unit> Units { get; }
    DbSet<DiningTable> DiningTables { get; }
    DbSet<Menu> Menus { get; }
    DbSet<MenuCategory> MenuCategories { get; }
    DbSet<MenuItem> MenuItems { get; }
    DbSet<MenuImport> MenuImports { get; }
    DbSet<Conversation> Conversations { get; }
    DbSet<ConversationMessage> ConversationMessages { get; }
    DbSet<GuardrailEvent> GuardrailEvents { get; }
    DbSet<AiModelSettings> AiModelSettings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
