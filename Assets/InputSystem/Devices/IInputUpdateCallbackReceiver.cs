namespace ISX
{
    public interface IInputUpdateCallbackReceiver
    {
        ////REVIEW: omit update type arg?
        void OnUpdate(InputUpdateType updateType);
    }
}
