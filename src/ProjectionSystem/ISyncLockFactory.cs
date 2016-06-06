using System.Threading.Tasks;

namespace ProjectionSystem {
  public interface ISyncLockFactory {
    Task<ISyncLock> Create();
  }
}