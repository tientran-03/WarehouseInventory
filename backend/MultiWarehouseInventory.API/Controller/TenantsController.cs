using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiWarehouseInventory.Application.DTOs;
using MultiWarehouseInventory.Application.Interfaces;
using MultiWarehouseInventory.Domain.Enums;

namespace MultiWarehouseInventory.API.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public class TenantsController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantsController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _tenantService.GetAllAsync();
            return Ok(result);
        }
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _tenantService.GetByIdAsync(id);
            return Ok(result);
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UpsertTenantRequest request)
        {
            var result = await _tenantService.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPost("{id:guid}/accounts")]
        public async Task<IActionResult> AddAccount(Guid id, [FromBody] CreateCompanyAccountRequest request)
        {
            await _tenantService.AddCompanyAccountAsync(id, request);
            return NoContent();
        }

        [HttpPost("{id:guid}/accounts/resend-verification")]
        public async Task<IActionResult> ResendVerification(Guid id, [FromBody] ResendVerificationEmailRequest request)
        {
            await _tenantService.ResendVerificationEmailAsync(id, request);
            return NoContent();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpsertTenantRequest request)
        {
            await _tenantService.UpdateAsync(id, request);
            return NoContent();
        }
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _tenantService.DeleteAsync(id);
            return NoContent();
        }
    }
}
