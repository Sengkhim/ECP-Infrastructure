using Microsoft.EntityFrameworkCore;

namespace ECPLibrary.Services;

public interface IConfigurationModeling
{
    void Configuration(ModelBuilder builder);
}