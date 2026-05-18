using CastleOps.Api.Infrastructure.Database.Repository;
using CastleOps.Core.Models;
using CastleOps.Core.DTOs;

namespace CastleOps.Api.Services;

public class PeonService
{
    private readonly IGenericRepository<Peon> _repo;
    private readonly IGenericRepository<PeonConfig> _peonConfigRepo;

    public PeonService(
        IGenericRepository<Peon> peonRepository,
        IGenericRepository<PeonConfig> peonConfigRepository)
    {
        _repo = peonRepository;
        _peonConfigRepo = peonConfigRepository;
    }

    public async Task<IEnumerable<PeonDTO>> GetAllPeonsAsync()
    {
        var peons = await _repo.GetAllAsync(p => p.Configs);
        return peons.Select(MapToDTO);
    }

    public async Task<PeonDTO?> GetPeonByIdAsync(Guid id)
    {
        var peon = await _repo.GetByIdAsync(id, p => p.Configs);
        return peon == null ? null : MapToDTO(peon);
    }

    public async Task<PeonDTO?> GetPeonBySlugAsync(string slug)
    {
        var peons = await _repo.FindAsync(p => p.Slug == slug);
        var peon = peons.FirstOrDefault();
        return peon == null ? null : MapToDTO(peon);
    }

    public async Task<PeonDTO> CreatePeonAsync(PeonDTO peonDTO)
    {
        var peon = new Peon
        {
            Id = peonDTO.Id == Guid.Empty ? Guid.NewGuid() : peonDTO.Id,
            DateCreated = DateTime.UtcNow,
            Slug = peonDTO.Slug,
            Name = peonDTO.Name,
            Entry = peonDTO.Entry,
            Url = peonDTO.Url,
            Type = peonDTO.Type,
            Description = peonDTO.Description,
            Author = peonDTO.Author,
            Tags = peonDTO.Tags ?? new List<string>(),
            DefaultVersion = peonDTO.DefaultVersion ?? "latest",
            DefaultEnvironment = peonDTO.DefaultEnvironment ?? new Dictionary<string, string>()
        };

        await _repo.CreateAsync(peon);
        return MapToDTO(peon);
    }

    public async Task UpdatePeonAsync(Guid id, PeonDTO updatedPeonDTO)
    {
        var peon = await _repo.GetByIdAsync(id);
        if (peon == null) return;

        peon.Slug = updatedPeonDTO.Slug;
        peon.Name = updatedPeonDTO.Name;
        peon.Url = updatedPeonDTO.Url;
        peon.Type = updatedPeonDTO.Type;
        peon.Description = updatedPeonDTO.Description;
        peon.Author = updatedPeonDTO.Author;
        peon.Tags = updatedPeonDTO.Tags;
        peon.Entry = updatedPeonDTO.Entry;

        await _repo.UpdateAsync(id, peon);
    }

    public async Task DeletePeonAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task<PeonConfig> AssignPeonToDeviceAsync(Guid peonId, Guid deviceId)
    {
        var peon = await _repo.GetByIdAsync(peonId)
            ?? throw new KeyNotFoundException($"Peon {peonId} not found");

        var config = new PeonConfig
        {
            Id = Guid.NewGuid(),
            DateCreated = DateTime.UtcNow,
            PeonId = peonId,
            DeviceId = deviceId,
            Version = peon.DefaultVersion,
            Environment = new Dictionary<string, string>(peon.DefaultEnvironment)
        };

        await _peonConfigRepo.CreateAsync(config);
        return config;
    }

    private static PeonDTO MapToDTO(Peon p) => new()
    {
        Id = p.Id,
        Slug = p.Slug,
        Name = p.Name,
        DateCreated = p.DateCreated,
        Url = p.Url,
        Type = p.Type,
        Description = p.Description,
        Author = p.Author,
        Tags = p.Tags,
        Entry = p.Entry,
        DefaultVersion = p.DefaultVersion,
        DefaultEnvironment = p.DefaultEnvironment,
        Configs = p.Configs.Select(c => new PeonConfigDTO
        {
            DeviceId = c.DeviceId,
            PeonId = c.PeonId,
            Version = c.Version,
            Environment = c.Environment
        }).ToList()
    };
}
