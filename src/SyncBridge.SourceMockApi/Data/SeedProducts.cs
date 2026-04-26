using SyncBridge.SourceMockApi.Models;

namespace SyncBridge.SourceMockApi.Data;

public static class SeedProducts
{
    public static List<SourceProductResponse> Get()
    {
        return new List<SourceProductResponse>
        {
            new() { Id = 1, Name = "Mechanical Keyboard",      Sku = "KB-001", Price = 1200000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T08:00:00Z").ToUniversalTime() },
            new() { Id = 2, Name = "Gaming Mouse",             Sku = "MS-002", Price =  950000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-24T09:30:00Z").ToUniversalTime() },
            new() { Id = 3, Name = "27 Inch Monitor",          Sku = "MN-003", Price = 4500000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T10:15:00Z").ToUniversalTime() },
            new() { Id = 4, Name = "Laptop Stand",             Sku = "LS-004", Price =  350000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T11:00:00Z").ToUniversalTime() },
            new() { Id = 5, Name = "USB-C Hub",                Sku = "HB-005", Price =  600000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T11:30:00Z").ToUniversalTime() },
            new() { Id = 6, Name = "Wireless Headphones",      Sku = "HP-006", Price = 2200000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T12:00:00Z").ToUniversalTime() },
            new() { Id = 7, Name = "Mechanical Keyboard Pro",  Sku = "KB-007", Price = 2500000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T12:30:00Z").ToUniversalTime() },
            new() { Id = 8, Name = "Gaming Chair",             Sku = "CH-008", Price = 3500000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T13:00:00Z").ToUniversalTime() },
            new() { Id = 9, Name = "Webcam Full HD",           Sku = "WC-009", Price =  900000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T13:30:00Z").ToUniversalTime() },
            new() { Id = 10, Name = "Microphone USB",          Sku = "MC-010", Price = 1500000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T14:00:00Z").ToUniversalTime() },
            new() { Id = 11, Name = "SSD 1TB",                 Sku = "SD-011", Price = 1800000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T14:30:00Z").ToUniversalTime() },
            new() { Id = 12, Name = "RAM 16GB DDR4",           Sku = "RM-012", Price = 1200000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T15:00:00Z").ToUniversalTime() },
            new() { Id = 13, Name = "Graphics Card RTX 4060",  Sku = "GPU-013", Price = 9500000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T15:30:00Z").ToUniversalTime() },
            new() { Id = 14, Name = "Power Supply 650W",       Sku = "PSU-014", Price = 1300000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T16:00:00Z").ToUniversalTime() },
            new() { Id = 15, Name = "PC Case Mid Tower",       Sku = "CS-015", Price =  900000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T16:30:00Z").ToUniversalTime() },
            new() { Id = 16, Name = "CPU Cooler",              Sku = "CL-016", Price =  700000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T17:00:00Z").ToUniversalTime() },
            new() { Id = 17, Name = "Laptop Dell XPS 13",      Sku = "LP-017", Price = 28000000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T17:30:00Z").ToUniversalTime() },
            new() { Id = 18, Name = "MacBook Air M2",          Sku = "LP-018", Price = 30000000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T18:00:00Z").ToUniversalTime() },
            new() { Id = 19, Name = "iPad Pro 11",             Sku = "TB-019", Price = 22000000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T18:30:00Z").ToUniversalTime() },
            new() { Id = 20, Name = "Smartphone Galaxy S23",   Sku = "PH-020", Price = 21000000, Currency = "VND", UpdatedAt = DateTime.Parse("2026-04-23T19:00:00Z").ToUniversalTime() }
        };
    }
}