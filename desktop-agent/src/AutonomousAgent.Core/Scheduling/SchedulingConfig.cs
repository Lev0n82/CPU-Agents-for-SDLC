namespace AutonomousAgent.Core.Scheduling
{
    public class SchedulingConfig
    {
        public NightlyRebootConfig NightlyReboot { get; set; } = new();
    }

    public class NightlyRebootConfig
    {
        public bool Enabled { get; set; } = true;
        public int Hour { get; set; } = 0;
        public int Minute { get; set; } = 0;
    }
}
