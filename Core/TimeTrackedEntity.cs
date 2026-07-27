namespace Keystone.Core;

public abstract class TimeTrackedEntity
{
    public DateTime Created { get; private set; }
    public DateTime Modified { get; private set; }
    public DateTime LastActive { get; private set; }

    public TimeTrackedEntity()
    {
        CreatedNow();
    }

    protected void CreatedNow()
    {
        Created = DateTime.UtcNow;
        Modified = DateTime.UtcNow;
        LastActive = DateTime.UtcNow;
    }

    public void ModifiedNow()
    {
        Modified = DateTime.UtcNow;
    }

    public void ActiveNow()
    {
        LastActive = DateTime.UtcNow;
    }
}
