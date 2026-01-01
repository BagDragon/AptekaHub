using AptekaInternetApp.Models.Tables;

namespace AptekaInternetApp.Models.ModelProgram
{
    public static class UserSession
    {
        public static int Id {  get; set; }
        public static string Login { get; set; }
        public static string Role { get; set; }
        public static string FIO {  get; set; }
        public static bool IsAdmin => Role == "Admin"; // Или ваша логика проверки на админа
    }
}
