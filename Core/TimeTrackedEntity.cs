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
        CreatedAt(DateTime.UtcNow);
    }

    public void ModifiedNow()
    {
        ModifiedAt(DateTime.UtcNow);
    }

    public void ActiveNow()
    {
        ActiveAt(DateTime.UtcNow);
    }

    public void CreatedAt(DateTime time)
    {
        Created = time;
        Modified = time;
        LastActive = time;
    }

    public void ModifiedAt(DateTime time)
    {
        Modified = time;
    }

    public void ActiveAt(DateTime time)
    {
        LastActive = time;
    }
}
