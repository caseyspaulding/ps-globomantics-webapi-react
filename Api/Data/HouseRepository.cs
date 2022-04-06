public interface IHouseRepository
{
    Task<List<HouseDto>> GetAll();
    Task<HouseDetailDto?> Get(int id);
    Task<HouseDto> Add(HouseDetailDto house);
    Task<HouseDto> Update(HouseDetailDto house);
    Task Delete(int id);
}

public class HouseRepository : IHouseRepository
{
    private readonly HouseDbContext context;

    private static HouseDto EntityToDto(HouseEntity e)
    {
        return new HouseDto(e.Id, e.Address, e.Country, e.Price);
    }

    private static HouseDetailDto EntityToDetailDto(HouseEntity e)
    {
        return new HouseDetailDto(e.Id, e.Address, e.Country, e.Description, e.Price, e.Photo,
            e.Bids.Select(b => new BidDto(b.Id, b.HouseId, b.Bidder, b.Amount)));
    }

    private static void DtoToEntity(HouseDetailDto dto, HouseEntity e)
    {
        e.Address = dto.Address;
        e.Country = dto.Country;
        e.Description = dto.Description;
        e.Price = dto.Price;
        e.Photo = dto.Photo;
    }

    public HouseRepository(HouseDbContext context)
    {
        this.context = context;
    }

    public async Task<List<HouseDto>> GetAll()
    {
        return await context.Houses.Select(e => EntityToDto(e)).ToListAsync();
    }

    public async Task<HouseDetailDto?> Get(int id)
    {
        var entity = await context.Houses.Include(h => h.Bids).SingleOrDefaultAsync(h => h.Id == id);
        if (entity == null)
            return null;
        return EntityToDetailDto(entity);
    }

    public async Task<HouseDto> Add(HouseDetailDto dto)
    {
        var entity = new HouseEntity();
        DtoToEntity(dto, entity);
        context.Houses.Add(entity);
        await context.SaveChangesAsync();
        return EntityToDto(entity);
    }

    public async Task<HouseDto> Update(HouseDetailDto dto)
    {
        var entity = await context.Houses.FindAsync(dto.Id);
        if (entity == null)
            throw new ArgumentException($"Trying to update house: entity with ID {dto.Id} not found.");
        DtoToEntity(dto, entity);
        context.Entry(entity).State = EntityState.Modified;
        await context.SaveChangesAsync();
        return EntityToDto(entity);
    }

    public async Task Delete(int id)
    {
        var entity = await context.Houses.FindAsync(id);
        if (entity == null)
            throw new ArgumentException($"Trying to delete house: entity with ID {id} not found.");
        context.Houses.Remove(entity);
        await context.SaveChangesAsync();
    }
}