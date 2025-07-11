using DKR.Core.Entities;
using DKR.Core.Interfaces;
using DKR.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace DKR.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        // Inject your DbContext or data source here
        private readonly DKRDbContext _context;

        public InventoryRepository(DKRDbContext context)
        {
            _context = context;
        }

        public async Task<InventoryItem> CreateAsync(InventoryItem inventoryItem)
        {
            _context.InventoryItems.Add(inventoryItem);
            await _context.SaveChangesAsync();
            return inventoryItem;
        }

        public async Task<InventoryItem> GetByIdAsync(string id)
        {
            var inventoryItem = await _context.InventoryItems.FindAsync(id);
            return inventoryItem ?? throw new ArgumentException($"InventoryItem with ID {id} not found", nameof(id));
        }


        public async Task<InventoryItem> UpdateAsync(InventoryItem inventoryItem)
        {
            _context.InventoryItems.Update(inventoryItem);
            await _context.SaveChangesAsync();
            return inventoryItem;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                var inventoryItem = await _context.InventoryItems.FindAsync(id);
                if (inventoryItem != null)
                {
                    _context.InventoryItems.Remove(inventoryItem);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<InventoryItem>> GetAllAsync()
        {
            return await _context.InventoryItems
                    .OrderByDescending(c => c.CreatedAt)
                    .ToListAsync();
        }
    }
}