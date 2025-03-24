namespace iChat.BackEnd.Services.Validators.TextMessageValidators
{

    public class TextMessageValidatorService
    {

        private readonly Dictionary<int,List<Func<string, bool>>> _bannedFilter = new();
        private readonly Dictionary<int,List<Func<string, string>>> _filter = new();
        public bool ApplyBannedFilters(int userType, string content)
        {
            if (_bannedFilter.TryGetValue(userType,out var funcs))
            {
                return _ApplyBannedFilter(funcs, content);
            }
            return true;
        }
        public string ApplyFilters(int userType, string content)
        {
            if (_filter.TryGetValue(userType, out var funcs))
            {
                return _ApplyFilter(funcs, content);
            }
            return content;
        }
        private bool _ApplyBannedFilter(List<Func<string,bool>> filters,string content)
        {
            foreach (var f in filters)
            {
                if (!f(content))
                    return false;
            }
            return true;
        }
        private string _ApplyFilter(List<Func<string,string>> filters,string content)
        {
            var result = string.Empty;
            foreach (var f in filters)
            {
                result = f(result);
            }
            return result;
        }
    }
}
