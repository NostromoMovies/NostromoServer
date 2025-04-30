namespace Nostromo.Server.Services;

public class SelectedProfileService
{
    private int? _selectedProfileId;
    private int? _userAge;

    public int? GetSelectedProfileId()
    {
        return _selectedProfileId;
    }

    public int? GetUserAge()
    {
        return _userAge;
    }

    public void SetSelectedProfileIdAge(int? selectedProfileId, int? userAge)
    {
        _selectedProfileId = selectedProfileId;
        _userAge = userAge;
    }
}