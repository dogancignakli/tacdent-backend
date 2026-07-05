using Tacdent.Core.Entities;
using Tacdent.Data.Context;
using Tacdent.Data.Repositories.Interfaces;

namespace Tacdent.Data.Repositories;

public class ConsentRepository(TacdentDbContext context)
    : Repository<Consent>(context), IConsentRepository;
