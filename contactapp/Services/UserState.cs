using ContactManager.Data;
using ContactManager.Models;
using ContactManager.Utilities;
using System;
using System.Threading.Tasks;

namespace ContactManager.Services
{
    public class UserState
    {
        private readonly UserService _userService;

        public UserState(UserService userService)
        {
            _userService = userService;
        }

        public User CurrentUser { get; private set; }

        public event Action OnChange;

        public async Task<(bool Success, string Error)> RegisterAsync(string userName, string email, string password)
        {
            var existing = await _userService.GetByEmailAsync(email);
            if (existing != null) return (false, "Email already registered");

            var byName = await _userService.GetByUserNameAsync(userName);
            if (byName != null) return (false, "Username already taken");

            var user = new User
            {
                UserName = userName,
                Email = email,
                PasswordHash = PasswordHasher.Hash(password)
            };

            await _userService.CreateAsync(user);
            CurrentUser = user;
            NotifyStateChanged();
            return (true, null);
        }

        public async Task<(bool Success, string Error)> LoginAsync(string userOrEmail, string password)
        {
            var user = await _userService.ValidateCredentialsAsync(userOrEmail, password);
            if (user == null) return (false, "Invalid credentials");

            CurrentUser = user;
            NotifyStateChanged();
            return (true, null);
        }

        public void Logout()
        {
            CurrentUser = null;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
