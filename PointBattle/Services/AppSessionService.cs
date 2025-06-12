namespace PointBattle.Services;

public class AppSessionService
{
    private bool _hasCheckedForRecovery = false;
    private bool _isFirstAppLaunch = true;

    public bool HasCheckedForRecovery => _hasCheckedForRecovery;
    public bool IsFirstAppLaunch => _isFirstAppLaunch;

    public void MarkRecoveryChecked()
    {
        _hasCheckedForRecovery = true;
        _isFirstAppLaunch = false;
    }

    public void ResetSession()
    {
        _hasCheckedForRecovery = false;
        _isFirstAppLaunch = true;
    }
}