using ECPLibrary.Persistent;
using ECPLibrary.Services;
using Microsoft.EntityFrameworkCore;

namespace ECPLibrary.Tests.DbContext;

public class FakeDbContext(
    DbContextOptions<FakeDbContext> options,
    IConfigurationModeling modeling)
    : EcpDatabase<FakeDbContext>(options, modeling);
