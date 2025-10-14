namespace Base
{
    public interface IEnvironmentStepper
    {
        void OnPrePhysicsStep();
        void OnPostPhysicsStep();
    }

    public interface IResettable
    {
        void OnReset();
    }
}