namespace Nostromo.Server.Services;

public class SelectedProfileService
{
    private int? _selectedProfileId;

    public int? GetSelectedProfileId()
    {
        return _selectedProfileId;
    }

    public void SetSelectedProfileId(int selectedProfileId)
    {
        _selectedProfileId = selectedProfileId;
    }
}