using CastleOps.Api.Infrastructure.Database.Repository;
using CastleOps.Core.Models;
using CastleOps.Core.DTOs;

namespace CastleOps.Api.Services;

public class PeonService
{
    private readonly IGenericRepository<Peon> _repo;
    private readonly IGenericRepository<PeonConfig> _peonConfigRepo;

    public PeonService(IGenericRepository<Peon> peonRepository, IGenericRepository<PeonConfig> peonConfigRepository)
    {
        _repo = peonRepository;
        _peonConfigRepo = peonConfigRepository;
    }

    public async Task<IEnumerable<PeonDTO>> GetAllPeonsAsync()
    {
        var peons = await _repo.GetAllAsync(p => p.Configs);
        return peons.Select(p => new PeonDTO
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
            Configs = p.Configs.Select(dc => new PeonConfigDTO
            {
                DeviceId = dc.DeviceId,
                PeonId = dc.PeonId,
                Version = dc.Version,
                Environment = dc.Environment
            }).ToList()
        });
    }

    public async Task<PeonDTO> GetPeonByIdAsync(Guid id)
    {
        var peon = await _repo.GetByIdAsync(id, p => p.Configs);
        if (peon == null) return null;

        return new PeonDTO
        {
            Id = peon.Id,
            Slug = peon.Slug,
            Name = peon.Name,
            DateCreated = peon.DateCreated,
            Url = peon.Url,
            Type = peon.Type,
            Description = peon.Description,
            Author = peon.Author,
            Tags = peon.Tags,
            Entry = peon.Entry,
            DefaultVersion = peon.DefaultVersion,
            DefaultEnvironment = peon.DefaultEnvironment,
            Configs = peon.Configs.Select(dc => new PeonConfigDTO
            {
                DeviceId = dc.DeviceId,
                PeonId = dc.PeonId,
                Version = dc.Version,
                Environment = dc.Environment
            }).ToList()
        };
    }

    public async Task<PeonDTO> CreatePeonAsync(PeonDTO peonDTO)
    {
        var peon = new Peon
        {
            Id = peonDTO.Id,
            Slug = peonDTO.Slug,
            Name = peonDTO.Name,
            DateCreated = DateTime.UtcNow,
            Entry = peonDTO.Entry,
            Url = peonDTO.Url,
            Type = peonDTO.Type,
            Description = peonDTO.Description,
            Author = peonDTO.Author,
            Tags = peonDTO.Tags,
            DefaultVersion = "latest",
            DefaultEnvironment = peonDTO.DefaultEnvironment
        };

        peon = await _repo.CreateAsync(peon);

        var createdPeon = new PeonDTO
        {
            Id = peon.Id,
            Slug = peon.Slug,
            Name = peon.Name,
            DateCreated = peon.DateCreated,
            Entry = peon.Entry,
            Url = peon.Url,
            Type = peon.Type,
            Description = peon.Description,
            Author = peon.Author,
            Tags = peon.Tags
        };

        return createdPeon;
    }

    public async Task UpdatePeonAsync(Guid id, PeonDTO updatedPeonDTO)
    {
        var existingPeon = await _repo.GetByIdAsync(id);
        if (existingPeon == null)
        {
            // Handle not found case
            return;
        }

        // Map updated fields from DTO to the existing entity
        existingPeon.Slug = updatedPeonDTO.Slug;
        existingPeon.Name = updatedPeonDTO.Name;
        existingPeon.Url = updatedPeonDTO.Url;
        existingPeon.Type = updatedPeonDTO.Type;
        existingPeon.Description = updatedPeonDTO.Description;
        existingPeon.Author = updatedPeonDTO.Author;
        existingPeon.Tags = updatedPeonDTO.Tags;

        await _repo.UpdateAsync(id, existingPeon);
    }

    public async Task DeletePeonAsync(Guid id)
    {
        await _repo.DeleteAsync(id);
    }

    public async Task<PeonConfig> AssignPeonToDeviceAsync(Guid peonId, Guid deviceId)
    {
        // 1. Get the Peon template, which contains the defaults
        var peonTemplate = await _repo.GetByIdAsync(peonId);
        if (peonTemplate == null)
        {
            throw new Exception("Peon not found.");
        }

        // 2. Create a new PeonConfig using the defaults from the template
        var newConfig = new PeonConfig
        {
            PeonId = peonId,
            DeviceId = deviceId,
            Version = peonTemplate.DefaultVersion, // Use default
            Environment = new Dictionary<string, string>(peonTemplate.DefaultEnvironment) // Create a copy
        };

        // 3. Save the new device-specific configuration
        await _peonConfigRepo.CreateAsync(newConfig);
        
        return newConfig;
    }
}