using DepotDirectApi.Models.DTOs;

namespace DepotDirectApi.Repositories;

public interface INoteRepository
{
    Task<NoteDto> CreateAsync(CreateNoteDto createNoteDto, int? createdBy = null);
    Task<NoteDto?> GetByIdAsync(int id);
    Task<IEnumerable<NoteDto>> GetByTargetAsync(int? siteId, int? depotId, int? parkingId, int? vehicleId);
    Task<IEnumerable<NoteDto>> GetByCompanyAsync(int companyId);
    Task<NoteDto?> UpdateStatusAsync(int id, UpdateNoteStatusDto updateDto, int? updatedBy = null);
}
